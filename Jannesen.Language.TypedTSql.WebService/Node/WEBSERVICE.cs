using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using LTTSQL = Jannesen.Language.TypedTSql;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.WebService.Node
{
    [LTTSQL.Library.DeclarationParser("WEBSERVICE")]
    public class WEBSERVICE: LTTSQL.Node.DeclarationService
    {
        public      readonly    WEBSERVICE_EMIT                 n_Emit;

        public                                                  WEBSERVICE(LTTSQL.Core.ParserReader reader, LTTSQL.Node.IParseContext parseContext): base(reader)
        {
            n_Emit = new WEBSERVICE_EMIT(reader, parseContext, this);
        }

        public      override    bool                            IsMember(LTTSQL.Node.DeclarationObjectCode entity)
        {
            return entity is WEBMETHOD || entity is WEBCOMPLEXTYPE;
        }

        public      override    void                            TranspileNode(Transpile.Context context)
        {
            n_Emit.TranspileNode(context);
        }
        public      override    void                            EmitDrop(StringWriter stringWriter)
        {
            n_Emit.EmitDrop(stringWriter);
        }
        public      override    bool                            EmitCode(EmitContext emitContext, SourceFile sourceFile)
        {
            return n_Emit.EmitCode(emitContext, sourceFile);
        }
        public      override    void                            EmitGrant(EmitContext emitContext, SourceFile sourceFile)
        {
            n_Emit.EmitGrant(emitContext, sourceFile);
        }
        public      override    void                            EmitServiceFiles(EmitContext emitContext, LTTSQL.Node.DeclarationServiceMethod[] methods, bool rebuild)
        {
            n_Emit.EmitServiceFiles(emitContext, methods, rebuild);
        }

        public      override    string                          CollapsedName()
        {
            return "webservice " + n_Name.n_EntitiyName.Name;
        }
    }
}
