using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using LTTS                 = Jannesen.Language.TypedTSql;
using LTTS_Core            = Jannesen.Language.TypedTSql.Core;

namespace Jannesen.VisualStudioExtension.TypedTSql.LanguageService
{
    internal class FileResult
    {

        public  readonly        ITextSnapshot                               Snapshot;
        public  readonly        IReadOnlyList<LTTS_Core.Token>              Tokens;
        public  readonly        IReadOnlyList<OutliningRegion>              OutliningRegions;
        public  readonly        IReadOnlyCollection<LTTS.TypedTSqlMessage>  Messages;

        public                                                              FileResult(ITextSnapshot snapshot, LTTS.SourceFile sourceFile)
        {
            this.Snapshot     = snapshot;
            this.Tokens       = sourceFile.Tokens;

            if (sourceFile.Declarations != null) {
                var     outliningRegions = new List<OutliningRegion>();

                foreach(var declaration in sourceFile.Declarations)
                    _outline_walker(declaration, outliningRegions);

                OutliningRegions = outliningRegions.Count > 0 ? outliningRegions : null;
            }

            if ((sourceFile.ParseMessages     != null && sourceFile.ParseMessages.Count     > 0) ||
                (sourceFile.TranspileMessages != null && sourceFile.TranspileMessages.Count > 0))
            {
                var messages = new List<LTTS.TypedTSqlMessage>();

                if (sourceFile.ParseMessages != null)
                    messages.AddRange(sourceFile.ParseMessages);

                if (sourceFile.TranspileMessages != null)
                    messages.AddRange(sourceFile.TranspileMessages);

                this.Messages = messages;
            }
        }

        private     static      void                                        _outline_walker(LTTS_Core.AstParseNode node, List<OutliningRegion> outliningRegions)
        {
            if (OutliningRegion.isSupported(node))
                outliningRegions.Add(new OutliningRegion(node));

            if (node.Children != null) {
                foreach(var c in node.Children) {
                    if (c is LTTS_Core.AstParseNode)
                        _outline_walker((LTTS_Core.AstParseNode)c, outliningRegions);
                }
            }
        }
    }
}
