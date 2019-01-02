using System;

namespace Jannesen.Language.TypedTSql.Transpile
{
    public class ContextRoot: Context
    {
        public      override    Context                             Parent                  { get { return null;                    } }
        public      override    ContextRoot                         RootContext             { get { return this;                    } }
        public      override    ContextBlock                        BlockContext            { get { return null;                    } }
        public      override    Transpiler                          Transpiler              { get { return _transpiler;             } }
        public      override    SourceFile                          SourceFile              { get { return _sourceFile;             } }
        public      override    GlobalCatalog                       Catalog                 { get { return _catalog;                } }
        public      override    Node.Node_ParseOptions              Options                 { get { return _options;                } }
        public      override    bool                                ReportNeedTranspile     { get { return _reportNeedTranspile;    } }
        public      override    Node.DeclarationEntity              DeclarationEntity       { get { return _declarationEntity;      } }
        public      override    DataModel.ISqlType                  ScopeIndentityType      { get { return _scopeIndentityType;  }
                                                                                              set { _scopeIndentityType = value; } }

        public                  int                                 BlockNr;
        public                  int                                 StoreTargetNr;

        private                 Transpiler                          _transpiler;
        private                 SourceFile                          _sourceFile;
        private                 GlobalCatalog                       _catalog;
        private                 Node.Node_ParseOptions              _options;
        private                 bool                                _reportNeedTranspile;
        private                 Node.DeclarationEntity              _declarationEntity;

        private                 DataModel.LabelList                 _labelList;
        private                 DataModel.CursorList                _cursorList;
        private                 DataModel.ISqlType                  _scopeIndentityType;

        internal                                                    ContextRoot(Transpiler transpiler, SourceFile sourceFile, GlobalCatalog catalog, Node.Node_ParseOptions options, bool reportNeedTranspile, Node.DeclarationEntity declarationEntity)
        {
            _transpiler          = transpiler;
            _sourceFile          = sourceFile;
            _catalog             = catalog;
            _options             = options;
            _reportNeedTranspile = reportNeedTranspile;
            _declarationEntity   = declarationEntity;
            BlockNr              = 0;
            StoreTargetNr        = 0;
        }

        public      override    void                                AddError(Core.IAstNode node, Exception err)
        {
            if (err is NeedsTranspileException && !_reportNeedTranspile)
                throw err;

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

        public                  DataModel.LabelList                 GetLabelList()
        {
            if (_labelList == null) {
                var labelList = new DataModel.LabelList();
                _parseLabelList(labelList, GetDeclarationObject<Node.DeclarationObjectCode>().n_Statement);
                _labelList = labelList;
            }

            return _labelList;
        }
        public                  DataModel.CursorList                GetCursorList()
        {
            if (_cursorList == null)
                _cursorList = new DataModel.CursorList(4);

            return _cursorList;
        }

        private                 void                                _parseLabelList(DataModel.LabelList labelList, Core.IAstNode node)
        {
            if (node is Node.Statement_label statementLabel) {
                var label = new DataModel.Label(statementLabel.n_Label.ValueString, statementLabel.n_Label);
                if (!labelList.TryAdd(label)) {
                    AddError(statementLabel.n_Label, "Label '" + statementLabel.n_Label.ValueString + "' already defined.");
                    return;
                }

                statementLabel.n_Label.SetSymbol(label);
            }
            else
            if (node.Children != null) {
                foreach (var c in node.Children)
                    _parseLabelList(labelList, c);
            }
        }
    }
}
