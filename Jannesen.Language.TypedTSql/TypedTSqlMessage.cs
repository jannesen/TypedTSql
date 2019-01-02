using System;
using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql
{
    public enum TypedTSqlMessageClassification
    {
        ParseError          = 1,
        TranspileError      = 2,
        TranspileWarning    = 3
    }

    public abstract class TypedTSqlMessage
    {
        public      abstract    TypedTSqlMessageClassification      Classification      { get; }
        public      readonly    SourceFile                          SourceFile;
        public      readonly    Library.FilePosition                Beginning;
        public      readonly    Library.FilePosition                Ending;
        public      readonly    string                              Message;
        public                  QuickFix                            QuickFix            { get; private set; }

        public                                                      TypedTSqlMessage(SourceFile sourceFile, string message)
        {
            SourceFile = sourceFile;
            Message    = message;

            System.Diagnostics.Debug.WriteLine(ToString());
        }
        public                                                      TypedTSqlMessage(SourceFile sourceFile, Core.IAstNode node, string message, QuickFix quickFix=null)
        {
            SourceFile     = sourceFile;

            var firstToken = node.GetFirstToken(Core.GetTokenMode.RemoveWhiteSpaceAndComment);
            var lastToken  = node.GetLastToken(Core.GetTokenMode.RemoveWhiteSpaceAndComment);

            if (firstToken != null)
                Beginning = firstToken.Beginning;

            if (lastToken != null)
                Ending = lastToken.Ending;

            Message  = message;
            QuickFix = quickFix;

            System.Diagnostics.Debug.WriteLine(ToString());
        }

        public      static      bool                                operator == (TypedTSqlMessage e1, TypedTSqlMessage e2)
        {
            if ((object)e1 == (object)e2) return true;
            if ((object)e1 == null || (object)e2 == null) return false;

            return e1.SourceFile == e2.SourceFile &&
                   e1.Message    == e2.Message    &&
                   e1.Beginning  == e2.Beginning  &&
                   e1.Ending     == e2.Ending;
        }
        public      static      bool                                operator != (TypedTSqlMessage e1, TypedTSqlMessage e2)
        {
            return !(e1 == e2);
        }
        public      override    int                                 GetHashCode()
        {
            return SourceFile.Filename.GetHashCode() ^ Message.GetHashCode() ^ Beginning.GetHashCode();
        }
        public      override    bool                                Equals(object obj)
        {
            if (obj is TypedTSqlMessage)
                return this == (TypedTSqlMessage)obj;

            return false;
        }
        public      override    string                              ToString()
        {
            string  rtn = SourceFile.Filename;

            if (Beginning.hasValue) {
                rtn += "(" + Beginning.ToString();

                if (Ending.hasValue)
                    rtn += "," + Ending.ToString();

                rtn += ")";
            }

            rtn += ": " + Message;

            return rtn;
        }

        public      static      string                              ErrorToString(Exception err)
        {
            string  msg = err.Message;

            for (err = err.InnerException ; err != null ; err = err.InnerException)
                msg += " " + err.Message;

            return msg;
        }
    }

    public class TypedTSqlParseError: TypedTSqlMessage
    {
        public      override    TypedTSqlMessageClassification      Classification { get { return TypedTSqlMessageClassification.ParseError; } }

        public                                                      TypedTSqlParseError(SourceFile sourceFile, string message): base(sourceFile, message)
        {
        }
        public                                                      TypedTSqlParseError(SourceFile sourceFile, Core.IAstNode node, string message): base(sourceFile, node, message)
        {
        }
        public                                                      TypedTSqlParseError(SourceFile sourceFile, Core.IAstNode node, Exception err): base(sourceFile, node, ErrorToString(err))
        {
        }
    }

    public class TypedTSqlTranspileWarning: TypedTSqlMessage
    {
        public      override    TypedTSqlMessageClassification      Classification { get { return TypedTSqlMessageClassification.TranspileWarning; } }

        public                                                      TypedTSqlTranspileWarning(SourceFile sourceFile, Core.IAstNode node, string message, QuickFix quickFix=null): base(sourceFile, node, message, quickFix)
        {
        }
    }

    public class TypedTSqlTranspileError: TypedTSqlMessage
    {
        public      override    TypedTSqlMessageClassification      Classification      { get { return TypedTSqlMessageClassification.TranspileError; } }


        public                                                      TypedTSqlTranspileError(SourceFile sourceFile, Core.IAstNode node, string message, QuickFix quickFix=null): base(sourceFile, node, message, quickFix)
        {
        }
        public                                                      TypedTSqlTranspileError(SourceFile sourceFile, Core.IAstNode node, Exception err): base(sourceFile, node, ErrorToString(err))
        {
        }
    }
}
