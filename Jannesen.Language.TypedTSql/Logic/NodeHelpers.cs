using System;
using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql.Library
{
    public static class NodeHelpers
    {
        public  static  void                TranspileNodes(this Core.AstParseNode[] nodes, Transpile.Context context)
        {
            foreach(var n in nodes) {
                try {
                    n.TranspileNode(context);
                }
                catch(Exception err) {
                    context.AddError(n, err);
                }
            }
        }
        public  static  void                TranspileNodes(this Node.IExprNode[] nodes, Transpile.Context context)
        {
            foreach(var n in nodes) {
                try {
                    n.TranspileNode(context);
                }
                catch(Exception err) {
                    context.AddError(n, err);
                }
            }
        }
    }
}
