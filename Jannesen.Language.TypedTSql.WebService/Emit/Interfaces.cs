using System;
using LTTSQL = Jannesen.Language.TypedTSql;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.WebService.Emit
{
    internal interface FileEmitor
    {
        void        AddWebMethod(Node.WEBMETHOD webMethod);
        void        AddIndexMethod(string pathname, string procedureName);
        void        Emit(EmitContext emitContext);
    }
}
