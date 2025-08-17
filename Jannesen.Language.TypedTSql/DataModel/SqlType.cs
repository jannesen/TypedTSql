using System;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public abstract class SqlType: ISqlType
    {
        public      virtual     SqlTypeFlags            TypeFlags       { get { return SqlTypeFlags.None;   } }
        public      virtual     SqlTypeNative           NativeType      { get { throw new InvalidOperationException(this.GetType().Name + ": has no nativetype.");      } }
        public      virtual     InterfaceList           Interfaces      { get { throw new InvalidOperationException(this.GetType().Name + ": has no interfaces.");      } }
        public      virtual     string                  TimeZone        { get { throw new InvalidOperationException(this.GetType().Name + ": has no time zone.");       } }
        public      virtual     object                  DefaultValue    { get { return null; } }
        public      virtual     ValueRecordList         Values          { get { throw new InvalidOperationException(this.GetType().Name + ": has no values.");          } }
        public      virtual     IColumnList             Columns         { get { throw new InvalidOperationException(this.GetType().Name + ": has no columns.");         } }
        public      virtual     IndexList               Indexes         { get { throw new InvalidOperationException(this.GetType().Name + ": has no indexes.");         } }
        public      virtual     JsonSchema              JsonSchema      { get { throw new InvalidOperationException(this.GetType().Name + ": has json-schema.");        } }
        public      virtual     Entity                  Entity          { get { return null;                                                                            } }
        public      virtual     ISqlType                ParentType      { get { return null;                                                                            } }

        public      virtual     string                  ToSql()
        {
            throw new InvalidOperationException("Can't get sql-type of " + this.GetType().Name + ".");
        }
    }
}
