using System;
using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql
{

    public class EmitError: Exception
    {
        public                  string          Filename            { get; private set; }
        public                  int?            LineNumber          { get; private set; }
        public                  int?            LinePos             { get; private set; }
        public      virtual     string          Code                { get { return null; } }
        public      virtual     bool            Warning             { get { return false; } }

        public                                  EmitError(string message): base(message)
        {
        }
        public                                  EmitError(string filename, int? lineNumber, int? linePos, string message): base(message)
        {
            this.Filename   = filename;
            this.LineNumber = lineNumber;
            this.LinePos    = linePos;
        }
    }

    public class EmitException: Exception
    {
        public                  object          Declaration         { get; private set; }

        public                                  EmitException(object declaration, string message): base(message)
        {
            this.Declaration = declaration;
        }
    }

    public class SqlError: EmitError
    {
        public      override    string          Code                { get { return "TSQL" + Number; } }
        public      override    bool            Warning             { get { return Class == 0;      } }

        public                  byte            Class               { get; protected set; }
        public                  int             Number              { get; protected set; }

        public                                  SqlError(string filename, int lineNumber, System.Data.SqlClient.SqlError sqlError): base(filename, lineNumber, null, sqlError.Message)
        {
            Class      = sqlError.Class;
            Number     = sqlError.Number;
        }
    }
}
