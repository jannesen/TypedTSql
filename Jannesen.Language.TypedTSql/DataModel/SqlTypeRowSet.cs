using System;

namespace Jannesen.Language.TypedTSql.DataModel
{
    public class SqlTypeRowSet: ISqlType
    {
        public                  SqlTypeFlags                TypeFlags       { get { return SqlTypeFlags.RowSet;   } }
        public                  SqlTypeNative               NativeType      { get { throw new InvalidOperationException(this.GetType().Name + ": has no nativetype.");      } }
        public                  InterfaceList               Interfaces      { get { throw new InvalidOperationException(this.GetType().Name + ": has no interfaces.");      } }
        public                  object                      DefaultValue    { get { return null; } }
        public                  ValueRecordList             Values          { get { throw new InvalidOperationException(this.GetType().Name + ": has no values.");          } }
        public                  IColumnList                 Columns         { get; private set; }
        public                  IndexList                   Indexes         { get { throw new InvalidOperationException(this.GetType().Name + ": has no indexes.");         } }
        public      virtual     JsonSchema                  JsonSchema      { get { throw new InvalidOperationException(this.GetType().Name + ": has no json-schema.");     } }
        public                  Entity                      Entity          { get { return null;                                                                            } }
        public      virtual     ISqlType                    ParentType      { get { return null;                                                                            } }
        public                  string                      ToSql()
        {
            throw new InvalidOperationException("Can't get sql-type of " + this.GetType().Name + ".");
        }

        public                                              SqlTypeRowSet(RowSet rowset)
        {
            this.Columns = rowset.Columns;
        }
    }
}
