using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LTTSQL = Jannesen.Language.TypedTSql;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.WebService.Node
{
    public abstract class WEBSERVICE_EMITOR: LTTSQL.Core.AstParseNode
    {
        internal abstract     Emit.FileEmitor             ConstructEmitor(string basedirectory);
    }
}
