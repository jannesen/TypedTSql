﻿using System;

namespace Jannesen.Language.TypedTSql.Token
{
    public class TokenLocalName: Core.TokenWithSymbol
    {
        public      override        Core.TokenID            ID
        {
            get {
                return Core.TokenID.LocalName;
            }
        }
        public      override        string                  ValueString
        {
            get {
                return Text;
            }
        }

        internal                                            TokenLocalName(Library.FilePosition beginning, Library.FilePosition ending, string text): base(beginning, ending, text)
        {
        }

        public      override        void                    Emit(Core.EmitWriter emitWriter)
        {
            string sqlName = null;

            if (SymbolData is DataModel.SymbolUsage symbolUsage) {
                sqlName = (symbolUsage.Symbol as DataModel.Variable)?.SqlName;
            }

            if (sqlName != null && sqlName != Text) {
                emitWriter.WriteText(sqlName);
            }
            else {
                emitWriter.WriteToken(this);
            }
        }

        public                      TokenLocalName          TokenName
        {
            get {
                return this;
            }
        }
    }
}
