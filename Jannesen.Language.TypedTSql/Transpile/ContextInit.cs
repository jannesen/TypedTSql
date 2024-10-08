﻿using System;

namespace Jannesen.Language.TypedTSql.Transpile
{
    public class ContextInit: Context
    {
        public      override    TranspileContext                    TranspileContext        { get { return _transpileContext;                                                                   } }
        public      override    Context                             Parent                  { get { throw new InvalidOperationException("Context.Parent not available in init.");               } }
        public      override    ContextRoot                         RootContext             { get { throw new InvalidOperationException("Context.RootContext not available in init.");          } }
        public      override    ContextBlock                        BlockContext            { get { throw new InvalidOperationException("Context.BlockContext not available in init.");         } }
        public      override    SourceFile                          SourceFile              { get { return _sourceFile;             } }
        public      override    Node.Node_ParseOptions              Options                 { get { throw new InvalidOperationException("Context.Options not available in init.");              } }
        public      override    bool                                ReportNeedTranspile     { get { throw new InvalidOperationException("Context.ReportNeedTranspile not available in init.");  } }
        public      override    Node.DeclarationEntity              DeclarationEntity       { get { throw new InvalidOperationException("Context.DeclarationEntity not available in init.");    } }
        public      override    DataModel.ISqlType                  ScopeIndentityType      { get { throw new InvalidOperationException("Context.ScopeIndentityType not available in init.");   }
                                                                                              set { throw new InvalidOperationException("Context.ScopeIndentityType not available in init.");   } }

        private                 TranspileContext                    _transpileContext;
        private                 SourceFile                          _sourceFile;

        internal                                                    ContextInit(TranspileContext transpileContext, SourceFile sourceFile)
        {
            _transpileContext = transpileContext;
            _sourceFile       = sourceFile;
        }

        public      override    void                                AddError(Core.IAstNode node, Exception err)
        {
            if (err is TranspileException)
                AddError(((TranspileException)err).Node, TypedTSqlTranspileError.ErrorToString(err), ((TranspileException)err).QuickFix);
            else
                AddError(node, TypedTSqlTranspileError.ErrorToString(err), null);
        }
        public      override    void                                AddError(Core.IAstNode node, string error, QuickFix quickFix=null)
        {
            _sourceFile.AddTranspileMessage(new TypedTSqlTranspileError(SourceFile, node, error, quickFix));
        }
        public      override    void                                AddWarning(Core.IAstNode node, string warning, QuickFix quickFix=null)
        {
            _sourceFile.AddTranspileMessage(new TypedTSqlTranspileWarning(SourceFile, node, warning, quickFix));
        }
    }
}
