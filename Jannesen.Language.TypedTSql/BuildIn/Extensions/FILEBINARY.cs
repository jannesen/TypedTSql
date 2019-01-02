using System;
using System.IO;
using System.Text;
using Jannesen.Language.TypedTSql.Node;

namespace Jannesen.Language.TypedTSql.BuildIn.Func
{
    //      filebinary ( string )
    public class FILEBINARY: ExprCalculationBuildIn
    {
        public      readonly    Core.Token                  n_Filename;
        public                  byte[]                      BinaryData          { get; private set; }

        public      override    DataModel.ValueFlags        ValueFlags          { get { return DataModel.ValueFlags.Const;                                                } }
        public      override    DataModel.ISqlType          SqlType             { get { return new DataModel.SqlTypeNative(DataModel.SystemType.VarBinary, maxLength:-1); } }

        internal                                            FILEBINARY(Internal.BuildinFunctionDeclaration declaration, Core.ParserReader reader): base(declaration, reader)
        {
            ParseToken(reader, Core.TokenID.LrBracket);
            n_Filename = ParseToken(reader, Core.TokenID.String);
            ParseToken(reader, Core.TokenID.RrBracket);
        }

        public      override    object                      ConstValue()
        {
            return BinaryData;
        }

        public      override    void                        TranspileNode(Transpile.Context context)
        {
            BinaryData = null;

            try {
                BinaryData = _filebinary(Path.Combine(Path.GetDirectoryName(context.SourceFile.Filename),  n_Filename.ValueString));
            }
            catch(Exception err) {
                context.AddError(n_Filename, err);
            }
        }
        public      override    void                        Emit(Core.EmitWriter emitWriter)
        {
            EmitCustom(emitWriter, (ew) => {
                            emitWriter.WriteValue(this.BinaryData);
                        });
        }

        private                 byte[]                      _filebinary(string filename)
        {
            byte[]      data;

            using (Stream stream = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read, 4096))
            {
                if (stream.Length > 4000000)
                    throw new TranspileException(this, "file to big.");

                data = new byte[stream.Length];

                if (stream.Read(data, 0, data.Length) != data.Length)
                    throw new TranspileException(this, "Readsize != filesize.");

                return data;
            }
        }
    }
}
