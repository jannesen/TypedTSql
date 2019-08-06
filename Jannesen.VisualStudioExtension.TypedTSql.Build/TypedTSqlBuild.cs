using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Build.Framework;
using LTTS            = Jannesen.Language.TypedTSql;
using LTTS_DataModel  = Jannesen.Language.TypedTSql.DataModel;
using Jannesen.VisualStudioExtension.TypedTSql.Build.Library;

namespace Jannesen.VisualStudioExtension.TypedTSql.Build
{
    public class TypedTSqlBuild: BaseTask
    {
        public      const       UInt32                  StatusMagic = 17636775;

        class UsingFile
        {
            public          Int32       id;
            public          Int32       BuildOrder;
            public          string      Filename;
            public          string      FilenameLower;
            public          bool        Removed;
            public          bool        Changed;

            public          bool        isTsql
            {
                get {
                    return FilenameLower.EndsWith(".sql", StringComparison.InvariantCultureIgnoreCase);
                }
            }
            public          bool        isTypedTSql
            {
                get {
                    return LTTS.SourceFile.isTypedTSqlFile(FilenameLower);
                }
            }

            public                      UsingFile(string filename)
            {
                this.BuildOrder    = int.MinValue;
                this.Filename      = filename;
                this.FilenameLower = filename.ToLowerInvariant();
                this.Removed       = true;
                this.Changed       = true;
            }

            public  static  int         Compare(UsingFile u1, UsingFile u2)
            {
                if (u1.BuildOrder != u2.BuildOrder)
                    return u1.BuildOrder - u2.BuildOrder;

                return string.Compare(u1.FilenameLower, u2.FilenameLower, StringComparison.InvariantCulture);
            }
        }

        [Required]
        public                  string                              DatabaseName            { get; set; }
        public                  ITaskItem[]                         SqlFiles                { get; set; }
        public                  string                              RebuildScript           { get; set; }
        public                  bool                                InitRebuildScript       { get; set; }
        public                  string                              Extensions              { get; set; }
        public                  string                              StatusFile              { get; set; }
        public                  bool                                DontEmitComment         { get; set; }
        public                  bool                                DontEmitCustomComment   { get; set; }

        private                 bool                                _incbuild;
        private                 LTTS.GlobalCatalog                  _typedtsqlcatalog;
        private                 List<UsingFile>                     _usingFiles;
        private                 Dictionary<string,UsingFile>        _usingDictionary;

        public                                                      TypedTSqlBuild()
        {
        }

        protected   override    bool                                Run()
        {
            try {
                bool            rtn = true;

                _typedtsqlcatalog = null;
                _usingFiles       = new List<UsingFile>();
                _usingDictionary  = new Dictionary<string,UsingFile>();

                if (String.IsNullOrEmpty(RebuildScript) &&
                    (!string.IsNullOrEmpty(StatusFile)) &&
                    File.Exists(FullFileName(StatusFile)))
                {
                    if (_loadStatus())
                        _incbuild = true;
                }
                else {
                    DeleteFile(StatusFile);
                    DeleteFile(RebuildScript);
                }

                _loadSqlFiles();

                SortedList<int, List<UsingFile>>        buildsteps = _processUsingFiles();

                if (buildsteps.Count > 0) {
                    Log.LogMessage(MessageImportance.Normal, "Database: " + DatabaseName);

                    using (LTTS.SqlDatabase database = new LTTS.SqlDatabase(DatabaseName))
                    {
                        if (!_incbuild) {
                            if (!string.IsNullOrEmpty(RebuildScript))
                                database.Output(FullFileName(RebuildScript));

                            if (InitRebuildScript)
                                database.InitRebuild();
                        }

                        foreach(KeyValuePair<int,List<UsingFile>> buildstep in buildsteps) {
                            buildstep.Value.Sort((u1, u2) => string.Compare(u1.FilenameLower, u2.FilenameLower, StringComparison.InvariantCulture));

                            if (rtn)
                                rtn = _processTSql(database, buildstep.Value);

                            if (rtn)
                                rtn = _processTypedTSql(database, buildstep.Value);
                        }
                    }

                    if (rtn) {
                        if (!string.IsNullOrEmpty(StatusFile))
                            _saveStatus();
                    }
                }

                return rtn;
            }
            catch(Exception err) {
                Log.LogErrorFromException(err);
                return false;
            }
        }

        private                 bool                                _loadStatus()
        {
            try {
                using (BinaryReader binaryReader = OpenStatusFile(StatusFile))
                {
                    if (binaryReader.ReadInt32() != StatusMagic)
                        throw new StatusFileException("Invalid status-file magic.");

                    if (binaryReader.ReadString() != this.GetType().Assembly.GetName().Version.ToString())
                        throw new StatusFileException("Invalid status-file version.");

                    for (int n = binaryReader.ReadInt32() ; n > 0 ; --n) {
                        UsingFile   usingFile = new UsingFile(binaryReader.ReadString());

                        usingFile.Changed = Statics.ReadFileStatus(binaryReader, usingFile.Filename);

                        _usingFiles.Add(usingFile);
                        _usingDictionary.Add(usingFile.FilenameLower, usingFile);
                    }
                }

                return true;
            }
            catch(Exception err) {
                if (!(err is FileNotFoundException || err is DirectoryNotFoundException))
                    Log.LogMessage(MessageImportance.High, this.GetType().Name + " LoadStatus failed. " + err.Message);

                return false;
            }
        }
        private                 void                                _loadSqlFiles()
        {
            try {
                foreach(ITaskItem sqlFile in SqlFiles) {
                    string  filename    = FullFileName(sqlFile.GetMetadata("FullPath"));
                    string  sbuildorder = sqlFile.GetMetadata("BuildOrder");
                    int     buildorder  = 1000;

                    if (!String.IsNullOrEmpty(sbuildorder))
                        int.TryParse(sbuildorder, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out buildorder);

                    UsingFile usingFile;

                    if (!_usingDictionary.TryGetValue(filename.ToLowerInvariant(), out usingFile)) {
                        _usingFiles.Add(usingFile = new UsingFile(filename));
                        _usingDictionary.Add(usingFile.FilenameLower, usingFile);
                    }

                    usingFile.BuildOrder = buildorder;
                    usingFile.Removed    = false;
                }
            }
            catch(Exception err) {
                throw new StatusFileException("LoadSqlFiles failed.", err);
            }
        }
        private                 SortedList<int, List<UsingFile>>    _processUsingFiles()
        {
            try {
                SortedList<int, List<UsingFile>>        rtn = new SortedList<int, List<UsingFile>>();

                _usingFiles.Sort(UsingFile.Compare);

                for (int i = 0 ; i < _usingFiles.Count ; ) {
                    UsingFile   usingFile = _usingFiles[i];

                    if (usingFile.Removed) {
                        _usingDictionary.Remove(usingFile.FilenameLower);
                        _usingFiles.RemoveAt(i);
                    }
                    else {
                        usingFile.id = i;

                        List<UsingFile> usingFiles;

                        if (!rtn.TryGetValue(usingFile.BuildOrder, out usingFiles))
                            rtn.Add(usingFile.BuildOrder, usingFiles = new List<UsingFile>());

                        usingFiles.Add(usingFile);

                        ++i;
                    }
                }

                return rtn;
            }
            catch(Exception err) {
                throw new BuildException("ProcessUsingFiles failed.", err);
            }
        }
        private                 void                                _saveStatus()
        {
            try {
                using (BinaryWriter binaryWriter = CreateStatusFile(StatusFile))
                {
                    binaryWriter.Write(StatusMagic);
                    binaryWriter.Write(this.GetType().Assembly.GetName().Version.ToString());
                    binaryWriter.Write((Int32)_usingFiles.Count);

                    foreach (UsingFile usingFile in _usingFiles)
                        Statics.SaveFileStatus(binaryWriter, usingFile.Filename);
                }
            }
            catch(Exception err) {
                Log.LogMessage(MessageImportance.High, this.GetType().Name + " SaveStatus failed. " + err.Message);
            }
        }

        private                 bool                                _processTSql(LTTS.SqlDatabase database, List<UsingFile> usingFiles)
        {
            int errcnt = 0;

            foreach (UsingFile usingFile in usingFiles) {
                if (usingFile.isTsql) {
                    if (usingFile.Changed || !_incbuild) {
                        var fullfilename = Path.Combine(ProjectDirectory, usingFile.Filename);
                        var filename = fullfilename;
                        if (filename.StartsWith(ProjectDirectory + "\\", StringComparison.InvariantCulture)) {
                            filename = filename.Substring(ProjectDirectory.Length + 1);
                        }
                        Log.LogMessage(MessageImportance.Normal, "Execute: " + filename);
                        database.Print("# EXECUTE '" + filename + "'...");
                        errcnt += database.ExecuteFile(fullfilename, _onEmitError);
                    }
                }
            }

            return errcnt == 0;
        }
        private                 bool                                _processTypedTSql(LTTS.SqlDatabase database, List<UsingFile> usingFiles)
        {
            LTTS.Transpiler transpiler = null;

            var     filenames = new List<string>();
            var     updatedFilenames = new HashSet<string>();

            foreach (UsingFile usingFile in usingFiles) {
                if (usingFile.isTypedTSql) {
                    if (usingFile.Changed || !_incbuild)
                        updatedFilenames.Add(usingFile.Filename);

                    filenames.Add(usingFile.Filename);
                }
            }

            if (updatedFilenames.Count == 0)
                return true;

            var start = DateTime.UtcNow;

            var catalog = (_typedtsqlcatalog == null) ? Task.Run(() => {
                                                                return new LTTS.GlobalCatalog(database);
                                                            }) : null;

            transpiler = new LTTS.Transpiler();
            transpiler.LoadExtensions(Extensions);
            transpiler.Parse(filenames.ToArray());
            _onEmitMessage("Parsing: " + (DateTime.UtcNow - start).TotalSeconds.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) + " sec.");

            if (catalog != null) {
                catalog.Wait();
                _typedtsqlcatalog = catalog.Result;
            }

            if (transpiler.ErrorCount == 0) {
                start = DateTime.UtcNow;
                var passes = transpiler.Transpile(_typedtsqlcatalog);
                _onEmitMessage("Transpiling: " + (passes > 1 ? passes + " passes " : "1 pass ") + (DateTime.UtcNow - start).TotalSeconds.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) + " sec.");
            }

            if (transpiler.ErrorCount == 0) {
                start = DateTime.UtcNow;

                transpiler.Emit(new LTTS.EmitOptions()
                                    {
                                        DontEmitComment       = this.DontEmitComment,
                                        DontEmitCustomComment = this.DontEmitCustomComment,
                                        BaseDirectory         = ProjectDirectory,
                                        OnEmitError           = _onEmitError,
                                        OnEmitMessage         = _onEmitMessage,
                                    },
                                database,
                                (_incbuild ? updatedFilenames : null));

                _onEmitMessage("Emiting: " + (DateTime.UtcNow - start).TotalSeconds.ToString("F2", System.Globalization.CultureInfo.InvariantCulture) + " sec.");

                return transpiler.EmitErrors.Count == 0;
            }

            foreach(var error in transpiler.Errors)
                _onTypedTSqlError(error);

            return false;
        }

        private                 void                                _onEmitMessage(string msg)
        {
            Log.LogMessage(MessageImportance.High, msg);
        }
        private                 void                                _onEmitError(LTTS.EmitError emitError)
        {
            if(emitError.Warning) {
                BuildEngine.LogWarningEvent(new BuildWarningEventArgs(null,
                                                                      emitError.Code,
                                                                      emitError.Filename,
                                                                      emitError.LineNumber ?? 0,
                                                                      emitError.LinePos    ?? 0,
                                                                      0,
                                                                      0,
                                                                      emitError.Message,
                                                                      "",
                                                                      ""));
            }
            else {
                BuildEngine.LogErrorEvent(new BuildErrorEventArgs(    null,
                                                                      emitError.Code,
                                                                      emitError.Filename,
                                                                      emitError.LineNumber ?? 0,
                                                                      emitError.LinePos    ?? 0,
                                                                      0,
                                                                      0,
                                                                      emitError.Message,
                                                                      "",
                                                                      ""));
            }
        }
        private                 void                                _onTypedTSqlError(LTTS.TypedTSqlMessage sqlError)
        {
            if (sqlError.Classification == Language.TypedTSql.TypedTSqlMessageClassification.TranspileWarning) {
                BuildEngine.LogWarningEvent(new BuildWarningEventArgs(null,
                                                                      null,
                                                                      sqlError.SourceFile.Filename,
                                                                      sqlError.Beginning.Lineno,
                                                                      sqlError.Beginning.Linepos,
                                                                      sqlError.Ending.Lineno,
                                                                      sqlError.Ending.Linepos,
                                                                      sqlError.Message,
                                                                      "",
                                                                      ""));
            }
            else {
                BuildEngine.LogErrorEvent(new BuildErrorEventArgs(    null,
                                                                      null,
                                                                      sqlError.SourceFile.Filename,
                                                                      sqlError.Beginning.Lineno,
                                                                      sqlError.Beginning.Linepos,
                                                                      sqlError.Ending.Lineno,
                                                                      sqlError.Ending.Linepos,
                                                                      sqlError.Message,
                                                                      "",
                                                                      ""));
            }
        }
    }
}
