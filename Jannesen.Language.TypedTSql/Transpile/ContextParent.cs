using System;

namespace Jannesen.Language.TypedTSql.Transpile
{
    public class ContextParent: Context
    {
        public      override    TranspileContext                    TranspileContext        { get { return _parent.TranspileContext;     } }
        public      override    Context                             Parent                  { get { return _parent;                      } }
        public      override    ContextRoot                         RootContext             { get { return _parent.RootContext;          } }
        public      override    ContextBlock                        BlockContext            { get { return _parent.BlockContext;         } }
        public      override    SourceFile                          SourceFile              { get { return _parent.SourceFile;           } }
        public      override    Node.Node_ParseOptions              Options                 { get { return _parent.Options;              } }
        public      override    bool                                ReportNeedTranspile     { get { return _parent.ReportNeedTranspile;  } }
        public      override    Node.DeclarationEntity              DeclarationEntity       { get { return _parent.DeclarationEntity;    } }
        public      override    Node.IDataTarget                    Target                  { get { return _parent.Target;               } }
        public      override    DataModel.QueryOptions              QueryOptions            { get { return _parent.QueryOptions;         } }
        public      override    DataModel.ISqlType                  ScopeIndentityType      { get { return _parent.ScopeIndentityType;   }
                                                                                              set { _parent.ScopeIndentityType = value;  } }

        protected               Context                             _parent;

        internal                                                    ContextParent(Context parent)
        {
            _parent       = parent;
        }

        public      override    void                                AddError(Core.IAstNode node, Exception err)
        {
            _parent.AddError(node, err);
        }
        public      override    void                                AddError(Core.IAstNode node, string error, QuickFix quickFix=null)
        {
            _parent.AddError(node, error, quickFix);
        }
        public      override    void                                AddWarning(Core.IAstNode node, string warning, QuickFix quickFix=null)
        {
            _parent.AddWarning(node, warning, quickFix);
        }
    }
}
