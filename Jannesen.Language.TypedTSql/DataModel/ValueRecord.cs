﻿using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class ValueRecord: ISymbol
    {
        public                  SymbolType          Type                    { get { return SymbolType.UDTValue; } }
        public                  string              Name                    { get ; private set; }
        public                  string              FullName                { get { return SqlStatic.QuoteName(Name); } }
        public      readonly    object              Value;
        public                  object              Declaration             { get ; private set; }
        public                  DataModel.ISymbol   ParentSymbol            { get { return null; } }
        public                  DataModel.ISymbol   SymbolNameReference     { get { return null; } }
        public      readonly    bool                Public;
        public      readonly    ValueFieldList      Fields;

        public                                      ValueRecord(string name, object value, object declaration, bool @public, ValueFieldList fields)
        {
            this.Name        = name;
            this.Value       = value;
            this.Declaration = declaration;
            this.Public      = @public;
            this.Fields      = fields;
        }
    }

    public class ValueRecordList: Library.ListHashName<ValueRecord>
    {
        public                                      ValueRecordList(int capacity): base(capacity)
        {
        }
        public                                      ValueRecordList(IReadOnlyList<ValueRecord> list): base(list)
        {
        }

        public                  bool                hasPublic()
        {
            foreach(var v in this) {
                if (v.Public) {
                    return true;
                }
            }

            return false;
        }

        protected   override    string              ItemKey(ValueRecord item)
        {
            return item.Name;
        }
    }

}
