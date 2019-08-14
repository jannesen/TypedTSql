using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.IO;
using SqlClient = System.Data.SqlClient;

namespace Jannesen.Language.TypedTSql
{
    public class SqlDatabase: IDisposable
    {
        public                  string                                  ServerName          { get; private set; }
        public                  string                                  DatabaseName        { get; private set; }
        public                  string                                  UserName            { get; private set; }
        public                  string                                  Passwd              { get; private set; }
        public                  bool                                    AllCodeDropped      { get; private set; }
        public                  SqlConnection                           Connection          { get; private set; }
        private                 Action<SqlError>                        _onExecuteError;
        private                 Action<string>                          _onExecuteMessage;
        private                 Library.SourceMap                       _sourceMap;
        private                 int                                     _errCnt;
        private                 TextWriter                              _output;
        private                 bool                                    _outputLeaveOpen;
        private                 bool                                    _needsResetSettings;

        public                                                          SqlDatabase(string datasource)
        {
            var s = datasource;
            int i;

            if ((i = s.IndexOf("@", StringComparison.Ordinal)) > 0) {
                var userPasswd = s.Substring(0, i);
                s = s.Substring(i + 1);

                if ((i = userPasswd.IndexOf(":", StringComparison.Ordinal)) <= 0)
                    throw new FormatException("Invalid username:passwd in datasource '" + datasource + "'");

                this.UserName = userPasswd.Substring(0, i);
                this.Passwd   = userPasswd.Substring(i + 1);
            }

            if ((i = s.LastIndexOf("\\", StringComparison.Ordinal)) <= 0)
                throw new FormatException("Invalid datasource '" + datasource + "'");

            this.ServerName    = s.Substring(0, i);
            this.DatabaseName  = s.Substring(i + 1);

            _connectionOpen();
        }
                                                                        ~SqlDatabase()
        {
            Dispose(false);
        }

        public                  void                                    Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected   virtual     void                                    Dispose(bool disposing)
        {
            if (disposing) {
                _connectionClose();
                _outputClose();
            }
        }

        public                  void                                    Output(string filename)
        {
            Output(new StreamWriter(filename, false, System.Text.Encoding.UTF8), false);
        }
        public                  void                                    Output(StreamWriter streamWriter, bool leaveOpen)
        {
            lock(this) {
                _outputClose();
                _output          = streamWriter;
                _outputLeaveOpen = leaveOpen;
            }
        }
        public                  void                                    InitRebuild()
        {
            lock(this) {
                if (_output != null) {
                    _output.Write("USE ");
                    _output.WriteLine(Library.SqlStatic.QuoteName(DatabaseName));
                    _output.WriteLine("GO");
                }

                ResetSettings();

                Print("# kill database connection");
                ExecuteScript("Jannesen.Language.TypedTSql.SqlScripts.KillConnections.sql");

                Print("# drop all code from database");
                ExecuteScript("Jannesen.Language.TypedTSql.SqlScripts.DropAllCode.sql");

                Print("# kill database connection");
                ExecuteScript("Jannesen.Language.TypedTSql.SqlScripts.KillConnections.sql");

                this.AllCodeDropped = true;
            }
        }
        public                  void                                    ResetSettings()
        {
            lock(this) {
                if (_needsResetSettings) {
                    try {
                        ExecuteStatement("SET NOCOUNT                 ON;\r\n" +
                                         "SET ANSI_WARNINGS           ON;\r\n" +
                                         "SET ANSI_NULLS              ON;\r\n" +
                                         "SET ANSI_PADDING            ON;\r\n" +
                                         "SET QUOTED_IDENTIFIER       ON;\r\n" +
                                         "SET CONCAT_NULL_YIELDS_NULL ON;\r\n" +
                                         "SET NUMERIC_ROUNDABORT      OFF;\r\n" +
                                         "SET LANGUAGE                US_ENGLISH;\r\n" +
                                         "SET DATEFORMAT              YMD;\r\n" +
                                         "SET DATEFIRST               7;\r\n" +
                                         "SET ARITHABORT              ON;");
                        _needsResetSettings = false;
                    }
                    catch(Exception err) {
                        throw new ErrorException("ResetSettings failed.", err);
                    }
                }
            }
        }
        public                  void                                    ExecuteStatement(string statement)
        {
            lock(this) {
                if (_output != null) {
                    _output.Write(statement);

                    if (!statement.EndsWith("\n", StringComparison.Ordinal))
                        _output.WriteLine();

                    _output.WriteLine("GO");
                }

                using (SqlClient.SqlCommand sqlCmd = new SqlClient.SqlCommand()) {
                    sqlCmd.CommandText    = statement;
                    sqlCmd.CommandType    = System.Data.CommandType.Text;
                    sqlCmd.Connection     = Connection;
                    sqlCmd.CommandTimeout = 30;

                    sqlCmd.ExecuteNonQuery();
                }
            }
        }
        public                  int                                     ExecuteStatement(string statement, Library.SourceMap sourceMap, Action<SqlError> onExecuteError, Action<string> onExecuteMessage=null)
        {
            lock(this) {
                _sourceMap        = sourceMap;
                _onExecuteError   = onExecuteError;
                _onExecuteMessage = onExecuteMessage;
                Connection.FireInfoMessageEventOnUserErrors = true;
                Connection.InfoMessage += _onInfoMessage;
                _errCnt = 0;

                try {
                    ExecuteStatement(statement);
                }
                catch(SqlClient.SqlException err) {
                    foreach(SqlClient.SqlError sqlError in err.Errors)
                        _executeError(sqlError);

                    throw new ErrorException("Fatal error executing statement.");
                }
                finally {
                    Connection.InfoMessage -= _onInfoMessage;
                    Connection.FireInfoMessageEventOnUserErrors = false;
                    _onExecuteError   = null;
                    _onExecuteMessage = null;
                    _sourceMap        = null;
                }

                return _errCnt;
            }
        }
        public                  int                                     ExecuteFile(string filename, string text, Action<SqlError> onExecuteError)
        {
            int         errcnt          = 0;

            lock(this) {
                ResetSettings();

                int         beginpos        = 0;
                int         lineoffset      = 1;

                while (beginpos < text.Length) {
                    int     endpos          = beginpos;
                    int     beginlineoffset = lineoffset;

                    while (endpos < text.Length) {
                        endpos = _nextLine(text, endpos);
                        ++lineoffset;

                        if (endpos < text.Length - 2) {
                            if ((text[endpos] == 'G' || text[endpos] == 'g') && (text[endpos + 1] == 'O' || text[endpos + 1] == 'o')) {
                                if (endpos < text.Length - 3) {
                                    if (text[endpos + 2] == ' ' || text[endpos + 2] == '\t' || text[endpos + 2] == '\r' || text[endpos + 2] == '\n')
                                        break;
                                }
                                else
                                    break;
                            }
                        }
                        else
                            break;
                    }

                    _needsResetSettings = true;

                    Library.SourceMap   sourceMap = new Library.SourceMap(filename, beginlineoffset);
                    sourceMap.AddFileRemap();
                    errcnt += ExecuteStatement(text.Substring(beginpos, endpos - beginpos), sourceMap, onExecuteError);

                    beginpos = _nextLine(text, endpos);
                    ++lineoffset;
                }
            }

            return errcnt;
        }
        public                  int                                     ExecuteFile(string filename, Action<SqlError> onExecuteError)
        {
            lock(this) {
                using (StreamReader streamReader = new StreamReader(filename))
                    return ExecuteFile(filename, streamReader.ReadToEnd(), onExecuteError);
            }
        }
        public                  void                                    ExecuteScript(string name)
        {
            lock(this) {
                try {
                    Stream stream = this.GetType().Assembly.GetManifestResourceStream(name);

                    if (stream == null)
                        throw new ErrorException("Can't open script from assembly.");

                    using (StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8))
                        ExecuteStatement(reader.ReadToEnd());
                }
                catch(Exception err) {
                    throw new ErrorException("ExecuteScript '" + name + "' failed.", err);
                }
            }
        }
        public                  void                                    Print(string text)
        {
            lock(this) {
                if (_output != null) {
                    _output.Write("PRINT ");
                    _output.Write(Library.SqlStatic.QuoteString(text));
                    _output.WriteLine(";");
                    _output.WriteLine("GO");
                }
            }
        }

        private                 void                                    _onInfoMessage(object sender, SqlClient.SqlInfoMessageEventArgs e)
        {
            foreach(SqlClient.SqlError sqlError in e.Errors) {
                if (sqlError.Class > 0)
                    _executeError(sqlError);
                else {
                    if (_onExecuteMessage != null)
                        _onExecuteMessage.Invoke(sqlError.Message);
                }
            }
        }
        private                 void                                    _executeError(System.Data.SqlClient.SqlError sqlClientError)
        {
            ++_errCnt;

            SqlError sqlError = (_sourceMap != null) ? new SqlError(_sourceMap.Filename, _sourceMap.RemapTargetToSource(sqlClientError.LineNumber), sqlClientError)
                                                     : new SqlError(null, sqlClientError.LineNumber, sqlClientError);

            try {
                _onExecuteError.Invoke(sqlError);
            }
            catch(Exception) {
            }
        }

        private                 void                                    _connectionOpen()
        {
            _connectionClose();

            try {
                var connectString = "Server="    + ServerName       +
                                    ";Database=" + DatabaseName     +
                                    ";Application Name=TypedTSql"   +
                                    ";Current Language=us_english"  +
                                    ";Connect Timeout=5"            +
                                    ";Pooling=false";

                if (UserName != null) {
                    connectString += ";User ID="  + UserName +
                                     ";Password=" + Passwd;
                }
                else
                    connectString += ";Integrated Security=true";

                Connection = new SqlClient.SqlConnection(connectString);
                Connection.Open();
                _needsResetSettings = true;
            }
            catch(Exception) {
                _connectionClose();
                throw;
            }
        }
        private                 void                                    _connectionClose()
        {
            if (Connection != null) {
                Connection.Close();
                Connection = null;
            }
        }
        private                 void                                    _outputClose()
        {
            if (_output != null) {
                if (!_outputLeaveOpen)
                    _output.Close();

                _output          = null;
                _outputLeaveOpen = false;
            }
        }
        private     static      int                                     _nextLine(string str, int startpos)
        {
            int     p = str.IndexOf('\n', startpos);
            if (p < 0)
                p = str.Length;

            return p + 1;
        }
    }
}
