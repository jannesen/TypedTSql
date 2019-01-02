using System;
using System.IO;
using System.Text;
using Microsoft.Build.Framework;

namespace Jannesen.VisualStudioExtension.TypedTSql.Build.Library
{
    public abstract class BaseTask: Microsoft.Build.Utilities.Task
    {
        [Required]
        public                  string                              ProjectDirectory    { get; set; }

        public      override    bool                                Execute()
        {
            bool    rtn;

            try {
                rtn = Run();
            }
            catch(Exception err) {
                Exception   firstException = err;;

                string msg = this.GetType().Name + " failed.";

                while (err != null) {
                    firstException = err;
                    msg += " " + err.Message;
                    err = err.InnerException;
                }

                msg += "\nStackTrace:\n" + firstException.StackTrace;
                Log.LogMessage(MessageImportance.High, msg);

                rtn = false;
            }

            if (!rtn)
                System.Threading.Thread.Sleep(50); // Work around bug in Visual studio where output is missing.

            return rtn;
        }

        protected   abstract    bool                                Run();
        protected               BinaryReader                        OpenStatusFile(string filename)
        {
            filename = FullFileName(filename);

            return new BinaryReader(System.IO.File.Open(filename, FileMode.Open));
        }
        protected               BinaryWriter                        CreateStatusFile(string filename)
        {
            filename = FullFileName(filename);

            CreatePath(filename);

            return new BinaryWriter(System.IO.File.Open(filename, FileMode.Create));
        }
        protected               string                              FullFileName(string filename)
        {
            return FullFileName(ProjectDirectory, filename);
        }
        protected   static      string                              FullFileName(string root, string filename)
        {
            filename = Path.Combine(root, filename.Replace("/", "\\"));

            if (filename.IndexOf("\\.") < 0)
                return filename;

            int         rootindex = _getRootIndex(filename);
            string[]    fileparts = filename.Substring(rootindex).Split('\\');
            string[]    resultparts = new string[fileparts.Length];

            int         rpos = 0;

            for (int i = 0 ; i < fileparts.Length ; ++i) {
                switch(fileparts[i]) {
                case "..":
                    if (rpos <= 0)
                        throw new BuildException("Invalid filename '" + filename + "'.");

                    --rpos;
                    break;

                case ".":
                    break;

                case "":
                    break;

                default:
                    resultparts[rpos++] = fileparts[i];
                    break;
                }
            }

            StringBuilder       rtn = new StringBuilder();

            rtn.Append(filename.Substring(0, rootindex).ToUpper());

            for (int i = 0 ; i < rpos ; ++i) {
                if (i > 0)
                    rtn.Append('\\');

                rtn.Append(resultparts[i]);
            }

            return rtn.ToString();
        }
        protected               void                                CreatePath(string filename)
        {
            try {
                _createPath(filename);
            }
            catch(Exception err) {
                throw new BuildException("Failed to create path for '" + filename + "'.", err);
            }
        }
        protected               void                                DeleteFile(string filename)
        {
            if (filename != null) {
                try {
                    File.Delete(FullFileName(filename));
                }
                catch(Exception err) {
                    if (!(err is FileNotFoundException || err is DirectoryNotFoundException))
                        throw new BuildException("Error deleting file '" + filename + "'.", err);
                }
            }
        }

        private                 void                                _createPath(string filename)
        {
            string dirpath  = Path.GetDirectoryName(filename);

            if (!Directory.Exists(dirpath)) {
                CreatePath(dirpath);
                Directory.CreateDirectory(dirpath);
            }
        }

        private     static      int                                 _getRootIndex(string filename)
        {
            if (filename.Length > 2 && filename[1] == ':'  && filename[2] == '\\')
                return 3;

            if (filename.StartsWith("\\")) {
                int     i = filename.IndexOf('\\', 2);
                if (i > 0) {
                    i = filename.IndexOf('\\', i + 1);

                    if (i > 0)
                        return i + 1;
                }
            }

            throw new BuildException("'" + filename + "' is not rooted.");
        }
    }
}
