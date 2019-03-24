using System;

namespace Jannesen.Language.TypedTSql.DataModel
{
    [Flags]
    public enum SqlTypeFlags
    {
        None            = 0,
        SimpleType      = 0x0001,
        UserType        = 0x0002,
        Interface       = 0x0004,
        Values          = 0x0008,
        Table           = 0x0010,
        RowSet          = 0x0020,
        Json            = 0x0040,
        ReponseNode     = 0x0080,
        CheckMode       = 0x0300,
        CheckTSql       = 0x0000,
        CheckSafe       = 0x0100,
        CheckStrong     = 0x0200,
        CheckStrict     = 0x0300,
        Flags           = 0x1000,
        RecVersion         = 0x2000
    }

    public interface ISqlType
    {
        SqlTypeFlags            TypeFlags           { get; }
        SqlTypeNative           NativeType          { get; }
        InterfaceList           Interfaces          { get; }
        object                  DefaultValue        { get; }
        ValueRecordList         Values              { get; }
        IColumnList             Columns             { get; }
        IndexList               Indexes             { get; }
        Entity                  Entity              { get; }
        JsonSchema              JsonSchema          { get; }

        string                  ToSql();
    }
}
