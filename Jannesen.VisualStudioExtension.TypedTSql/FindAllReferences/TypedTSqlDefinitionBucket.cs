using System;
using Microsoft.VisualStudio.Shell.FindAllReferences;
using Microsoft.VisualStudio.Shell.TableManager;
using LTTS                 = Jannesen.Language.TypedTSql;

namespace Jannesen.VisualStudioExtension.TypedTSql.FindAllReferences
{
    internal class TypedTSqlDefinitionBucket: DefinitionBucket
    {
        public  readonly        string          Text;

        public                                  TypedTSqlDefinitionBucket(LTTS.DataModel.ISymbol symbol): base(symbol.FullName, "TypedTSqlSource", "TypedTSqlIdentifier")
        {
            Text = this.Name;
        }

        public  override        bool            TryGetValue(string keyName, out object content)
        {
            content = _getValue(keyName);
            return content != null;
        }

        private                 object          _getValue(string keyName)
        {
            switch(keyName) {
            case StandardTableKeyNames.Text:
            case StandardTableKeyNames.FullText:
                return Text;

            default:
                return null;
            }
        }
    }
}
