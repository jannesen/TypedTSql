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

                if (endNode.Text.EndsWith("\n", StringComparison.Ordinal))
                    _ending -= endNode.Text.EndsWith("\r\n", StringComparison.Ordinal) ? 2 : 1;
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
    }
}
