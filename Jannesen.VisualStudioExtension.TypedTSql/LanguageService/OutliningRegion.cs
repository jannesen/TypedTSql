using System;
using Microsoft.VisualStudio.Text.Tagging;
using LTTS_Core       = Jannesen.Language.TypedTSql.Core;
using LTTS_Node       = Jannesen.Language.TypedTSql.Node;
using LTTS_DataModel  = Jannesen.Language.TypedTSql.DataModel;

namespace Jannesen.VisualStudioExtension.TypedTSql.LanguageService
{
    internal class OutliningRegion: IOutliningRegionTag, ITag
    {
        private         LTTS_Core.IAstNode  _node;
        private         int                 _beginning;
        private         int                 _ending;
        private         string              _collapsedForm;

        public                              OutliningRegion(LTTS_Core.IAstNode node)
        {
            _node = node;

            LTTS_Core.Token startNode = node.GetFirstToken(LTTS_Core.GetTokenMode.RemoveWhiteSpace);

            if (startNode != null) {
                _beginning = startNode.Beginning.Filepos;

                LTTS_Core.Token endNode = node.GetLastToken(LTTS_Core.GetTokenMode.RemoveWhiteSpace);
                _ending = endNode.Ending.Filepos;

                if (endNode.Text.EndsWith("\n", StringComparison.InvariantCulture))
                    _ending -= endNode.Text.EndsWith("\r\n", StringComparison.InvariantCulture) ? 2 : 1;
            }
        }

        public  static  bool                isSupported(LTTS_Core.IAstNode node)
        {
            return node is LTTS_Node.Declaration         ||
                   node is LTTS_Node.Statement_BEGIN_END ||
                   node is LTTS_Node.Statement_TRY_CATCH;
        }

        public          int                 Beginning                   { get => _beginning; }
        public          int                 Ending                      { get => _ending;    }
        public          object              CollapsedForm
        {
            get {
                if (_collapsedForm == null)
                    _collapsedForm = _typedtsqlCollapsedForm(_node);

                return _collapsedForm;
            }
        }
        public          object              CollapsedHintForm
        {
            get {
                return "todo";
            }
        }
        public          bool                IsDefaultCollapsed          { get => true; }
        public          bool                IsImplementation            { get => _node is LTTS_Node.Declaration; }

        private static  string              _typedtsqlCollapsedForm(LTTS_Core.IAstNode node)
        {
            if (node is LTTS_Node.Declaration declaration)      return declaration.CollapsedName();
            if (node is LTTS_Node.Statement_BEGIN_END)          return "begin-end";
            if (node is LTTS_Node.Statement_TRY_CATCH)          return "try-catch";

            return node.GetType().Name;
        }
        private static  string              _typedtsqlobjecttypeToString(LTTS_DataModel.SymbolType type)
        {
            switch(type) {
            case LTTS_DataModel.SymbolType.Assembly:                            return "assembly";
            case LTTS_DataModel.SymbolType.TypeUser:                            return "type";
            case LTTS_DataModel.SymbolType.Default:                             return "default";
            case LTTS_DataModel.SymbolType.Rule:                                return "rule";
            case LTTS_DataModel.SymbolType.TypeTable:                           return "table-type";
            case LTTS_DataModel.SymbolType.TableInternal:
            case LTTS_DataModel.SymbolType.TableSystem:
            case LTTS_DataModel.SymbolType.TableUser:                           return "table";
            case LTTS_DataModel.SymbolType.Constraint_ForeignKey:
            case LTTS_DataModel.SymbolType.Constraint_PrimaryKey:
            case LTTS_DataModel.SymbolType.Constraint_Check:
            case LTTS_DataModel.SymbolType.Constraint_Unique:                   return "constraint";
            case LTTS_DataModel.SymbolType.View:                                return "view";
            case LTTS_DataModel.SymbolType.Function:
            case LTTS_DataModel.SymbolType.FunctionScalar:
            case LTTS_DataModel.SymbolType.FunctionScalar_clr:
            case LTTS_DataModel.SymbolType.FunctionInlineTable:
            case LTTS_DataModel.SymbolType.FunctionMultistatementTable:
            case LTTS_DataModel.SymbolType.FunctionMultistatementTable_clr:
            case LTTS_DataModel.SymbolType.FunctionAggregateFunction_clr:       return "function";
            case LTTS_DataModel.SymbolType.StoredProcedure:
            case LTTS_DataModel.SymbolType.StoredProcedure_clr:
            case LTTS_DataModel.SymbolType.StoredProcedure_extended:            return "procedure";
            case LTTS_DataModel.SymbolType.Trigger:
            case LTTS_DataModel.SymbolType.Trigger_clr:                         return "trigger";
            case LTTS_DataModel.SymbolType.PlanGuide:                           return "plan-guide";
            case LTTS_DataModel.SymbolType.ReplicationFilterProcedure:          return "filter-procedure";
            case LTTS_DataModel.SymbolType.SequenceObject:                      return "sequence-object";
            case LTTS_DataModel.SymbolType.ServiceQueue:                        return "service-queue";
            case LTTS_DataModel.SymbolType.Synonym:                             return "synonym";
            case LTTS_DataModel.SymbolType.ServiceMethod:                       return "service-method";
            default:                                                            return type.ToString();
            }
        }
    }
}
