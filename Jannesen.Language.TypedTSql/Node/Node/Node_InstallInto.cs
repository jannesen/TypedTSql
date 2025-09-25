using System;
using System.Collections.Generic;
using System.Text;
using Jannesen.Language.TypedTSql.Logic;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    public class Node_InstallInto: Core.AstParseNode
    {
        [Flags]
        public enum Option
        {
            Insert          = 0x01,
            Update          = 0x02,
            Delete          = 0x04
        }
        public      readonly    Node_EntityNameReference        n_Table;
        public      readonly    Option                          n_Options;

        private                 DataModel.EntityObjectTable     _table;
        private                 int[]                           _columnIndexes;

        public                                                  Node_InstallInto(Core.ParserReader reader)
        {
            ParseToken(reader, "INSTALL");
            ParseToken(reader, Core.TokenID.INTO);
            n_Table = AddChild(new Node_EntityNameReference(reader, EntityReferenceType.Table, DataModel.SymbolUsageFlags.Select));

            if (ParseOptionalToken(reader, Core.TokenID.OPTION) != null) {
                ParseToken(reader, Core.TokenID.LrBracket);
                do {
                    n_Options |= _parseEnum.Parse(this, reader);
                }
                while (ParseOptionalToken(reader, Core.TokenID.Comma) != null);

                ParseToken(reader, Core.TokenID.RrBracket);
            }
            else
                n_Options = Option.Insert | Option.Update | Option.Delete;
        }
        public      override    void                            TranspileNode(Transpile.Context context)
        {
            n_Table.TranspileNode(context);
        }
        public                  void                            TranspileNode(TypeDeclaration_User typeDeclaration, Transpile.Context context)
        {
            if (n_Table.Entity is DataModel.EntityObjectTable table) {
                var primaryKey = table.Indexes?.PrimaryKey;
                if (primaryKey == null || primaryKey.Columns.Length != 1 || primaryKey.Columns[0].Column != table.Columns[0]) {
                    context.AddError(n_Table, "Table has no primary key or primary key not the first column.");
                    return;
                }

                if (table.Columns[0].SqlType != typeDeclaration.Entity) {
                    context.AddError(n_Table, "Invalid first columns type. Type is " + table.Columns[0].SqlType.ToString() + " needs " + typeDeclaration.Entity + ".");
                    return;
                }

                var values = typeDeclaration.n_Values;
                if (values == null) {
                    context.AddError(n_Table, "No value to install.");
                    return;
                }

                var columns = new bool[table.Columns.Count];

                foreach (var r in values.n_Records) {
                    try {
                        if (r.n_Fields != null) {
                            bool    ferror = false;
                            var     reccolumn = new bool[table.Columns.Count];

                            foreach (var f in r.n_Fields) {
                                var columnIndex = table.ColumnList.IndexOf(f.n_Name.ValueString);
                                if (columnIndex > 0) {
                                    if (reccolumn[columnIndex])
                                        context.AddError(f.n_Name, "Duplicate field definition.");

                                    reccolumn[columnIndex] = true;
                                    columns[columnIndex] = true;

                                    var column = table.Columns[columnIndex];
                                    f.n_Name.SetSymbolUsage(column, DataModel.SymbolUsageFlags.Write);
                                    context.CaseWarning(f.n_Name, column.Name);

                                    Validate.ConstByType(column.SqlType, f.n_Value);
                                }
                                else {
                                    context.AddError(f.n_Name, "Field name don't exists in " + table.Name.ToString(System.Globalization.CultureInfo.InvariantCulture) + ".");
                                    ferror = true;
                                }
                            }

                            if (!ferror) {
                                for (int columnIndex = 1 ; columnIndex < table.ColumnList.Count ; ++columnIndex) {
                                    if (!reccolumn[columnIndex] && !table.ColumnList[columnIndex].isNullable) {
                                        context.AddError(r, "Missing column " + table.ColumnList[columnIndex].Name + ".");
                                    }
                                }
                            }
                        }
                    }
                    catch(Exception err) {
                        context.AddError(r, err);
                    }
                }

                var columnIndexes = new List<int>();

                for (int c = 1 ; c < table.ColumnList.Count ; ++c) {
                    if (columns[c])
                        columnIndexes.Add(c);
                }

                _table         = table;
                _columnIndexes = columnIndexes.ToArray();
            }
        }

        public                  bool                            EmitInstallInto(EmitContext emitContext, DataModel.ValueRecordList values, int step)
        {
            return (new EmitInstallIntoHelper(_table, _columnIndexes, n_Options, values)).Emit(emitContext, step);
        }

        private static  Core.ParseEnum<Option>                  _parseEnum = new Core.ParseEnum<Option>(
                                                                                    "Install Into option",
                                                                                    new Core.ParseEnum<Option>.Seq(Option.Insert,   Core.TokenID.INSERT),
                                                                                    new Core.ParseEnum<Option>.Seq(Option.Update,   Core.TokenID.UPDATE),
                                                                                    new Core.ParseEnum<Option>.Seq(Option.Delete,   Core.TokenID.DELETE)
                                                                                );

        class EmitInstallIntoHelper
        {
            private             DataModel.ColumnList        _columnList;
            private             string                      _keyName;
            private             bool                        _identityInsert;
            private             string                      _tableFullname;
            private             int[]                       _columnIndexes;
            private             Option                      _options;
            private             DataModel.ValueRecordList   _values;
            private             StringBuilder               _emitString;

            public                                          EmitInstallIntoHelper(DataModel.EntityObjectTable table, int[] columIndexes, Option options, DataModel.ValueRecordList values)
            {
                _columnList     = table.ColumnList;
                _keyName        = Library.SqlStatic.QuoteName(_columnList[0].Name);
                _identityInsert = _columnList[0].isIdentity;
                _tableFullname  = table.EntityName.Fullname;
                _columnIndexes  = columIndexes;
                _options        = options;
                _values         = values;
                _emitString     = new StringBuilder(4096);
            }

            public              bool                        Emit(EmitContext emitContext, int step)
            {
                bool   rtn  = true;
                string msg  = null;

                switch(step) {
                case 1:
                    if (!_emit_1())
                        return true;

                    msg  = "install data into ";
                    break;

                case 2:
                    _emit_2();
                    msg  = "restore checks ";
                    break;
                }

                bool first = true;
                emitContext.Database.Print("# " + msg.PadRight(38, ' ') + _tableFullname);
                emitContext.Database.ExecuteStatement(_emitString.ToString(), null,
                                                      (e) => {
                                                          emitContext.AddEmitError(new EmitError(msg + _tableFullname + " failed: " + e.Message));
                                                          rtn = false;
                                                      },
                                                      (m) => {
                                                          if (first) {
                                                              emitContext.AddEmitMessage(msg + _tableFullname);
                                                              first = false;
                                                          }
                                                          emitContext.AddEmitMessage("SQL: " + m);
                                                      });

                return rtn;
            }

            private             bool                        _emit_1()
            {
                if (_identityInsert)
                    _emitLine("DECLARE @identity_on BIT = 0;");

                _emitLine("DECLARE @cmd    NVARCHAR(4000);");
                _emitLine("DECLARE ccmd CURSOR LOCAL STATIC FOR");
                _emitLine("SELECT [cmd]");
                _emitLine("  FROM (");

                if (!_emit_1_2(12))
                    return false;

                _emitLine("       ) x");
                _emitLine("  WHERE [cmd] IS NOT NULL");
                _emitLine(" ORDER BY [p], [key]");
                _emitLine("OPEN ccmd;");
                _emitLine("FETCH ccmd INTO @cmd;");
                _emitLine("WHILE @@fetch_status = 0");
                _emitLine("BEGIN");
                    _emit("    ALTER TABLE  ");
                    _emit(_tableFullname);
                    _emit(" NOCHECK CONSTRAINT ALL;");
                    _emitNewline();
                _emitLine("    PRINT @cmd;");
                if (_identityInsert) {
                    _emitLine("    IF @CMD LIKE N'INSERT %'");
                    _emitLine("    BEGIN");
                    _emitLine("        IF @identity_on=0");
                    _emitLine("        BEGIN");
                        _emit("            SET IDENTITY_INSERT ");
                        _emit(_tableFullname);
                        _emit(" ON;");
                        _emitNewline();
                    _emitLine("            SET @identity_on=1;");
                    _emitLine("        END");
                    _emitLine("    END");
                }
                _emitLine("    EXECUTE(@cmd);");
                _emitLine("    FETCH ccmd INTO @cmd;");
                _emitLine("END");
                _emitLine("DEALLOCATE ccmd;");

                if (_identityInsert) {
                    _emitLine("IF @identity_on=1");
                    _emitLine("BEGIN");
                            _emit("    SET IDENTITY_INSERT ");
                            _emit(_tableFullname);
                            _emit(" OFF");
                            _emitNewline();
                        _emit("    DECLARE @n int = (SELECT MAX(");
                                _emit(_keyName);
                                _emit(") FROM ");
                                _emit(_tableFullname);
                                _emit(")");
                                _emitNewline();
                        _emit("    DBCC checkident('");
                                _emit(_tableFullname);
                                _emit("', 'RESEED', @n) WITH NO_INFOMSGS");
                                _emitNewline();
                    _emitLine("END");
                }

                return true;
            }
            private             void                        _emit_2()
            {
                _emit("ALTER TABLE  ");
                    _emit(_tableFullname);
                    _emit(" CHECK CONSTRAINT ALL;");
                    _emitNewline();
            }
            private             bool                        _emit_1_2(int indent)
            {
                _emitSpace(indent);
                _emit("SELECT [cmd] = CASE WHEN c.");
                    _emit(_keyName);
                    _emit(" IS NULL");
                    _emitNewline();

                _insert(indent);

                if ((_options & Option.Delete) == Option.Delete) {
                    _emitSpace(indent + 20);
                    _emit("WHEN n.");
                        _emit(_keyName);
                        _emit(" IS NULL");
                        _emitNewline();

                    _delete(indent);
                }

                if ((_options & Option.Update) == Option.Update) {
                    _testUpdate(indent);
                    _update(indent);
                }

                _emitSpace(indent + 15);
                    _emit("END,");
                    _emitNewline();

                _emitSpace(indent + 7);
                _emit("[p]   = CASE WHEN c.");
                    _emit(_keyName);
                    _emit(" IS NULL");
                    _emitNewline();

                _emitSpace(indent + 25);
                    _emit("THEN 1");
                    _emitNewline();

                _emitSpace(indent + 20);
                    _emit("WHEN n.");
                    _emit(_keyName);
                    _emit(" IS NULL");
                    _emitNewline();

                _emitSpace(indent + 25);
                    _emit("THEN 3");
                    _emitNewline();

                _emitSpace(indent + 25);
                    _emit("ELSE 2");
                    _emitNewline();

                _emitSpace(indent + 15);
                    _emit("END,");
                    _emitNewline();

                _emitSpace(indent + 7);
                    _emit("[key] = ISNULL(n.");
                    _emit(_keyName);
                    _emit(", c.");
                    _emit(_keyName);
                    _emit(")");
                    _emitNewline();

                _emitSpace(indent + 2);
                    _emit("FROM (");
                    _emitNewline();

                if (_valueDataSet(indent + 12) == 0)
                    return false;

                _emitSpace(indent + 7);
                    _emit(") n");
                    _emitNewline();

                _emitSpace(indent + 7);
                    _emit((_options & Option.Delete) == Option.Delete ? "FULL" : "LEFT");
                    _emit(" OUTER JOIN ");
                    _emit(_tableFullname);
                    _emit(" c ON c.");
                    _emit(_keyName);
                    _emit(" = n.");
                    _emit(_keyName);
                    _emitNewline();

                return true;
            }
            private             void                        _insert(int indent)
            {
                _emitSpace(indent + 25);
                _emit("THEN N'INSERT INTO ");
                    _emit(_tableFullname);
                    _emit("(");
                    _emit(_keyName);

                    foreach(int columnIndex in _columnIndexes) {
                        _emit(", ");
                        _emit(Library.SqlStatic.QuoteName(_columnList[columnIndex].Name));
                    }

                    _emit(")' +");
                    _emitNewline();

                _emitSpace(indent + 34);
                    _emit("N' VALUES(' + ");
                    _emit(_strNewValue("n.", _columnList[0]));

                    foreach(int columnIndex in _columnIndexes) {
                        _emit(" + N', ' + ");
                        _emit(_strNewValue("n.", _columnList[columnIndex]));
                    }

                    _emit("+ N')'");
                    _emitNewline();

            }
            private             void                        _testUpdate(int indent)
            {
                bool    first = true;

                foreach(int columnIndex in _columnIndexes) {
                    var column     = _columnList[columnIndex];
                    var columnName = Library.SqlStatic.QuoteName(column.Name);

                    _emitSpace(indent + 20);

                    if (first) {
                        _emit("WHEN ");
                        first = false;
                    }
                    else
                        _emit("  OR ");

                    _emit("(");
                        _emit("n.");
                        _emit(columnName);
                        _emit(" <> ");
                        _emit("c.");
                        _emit(columnName);
                        switch(column.SqlType.NativeType.SystemType) {
                        case DataModel.SystemType.VarChar:
                        case DataModel.SystemType.NVarChar:
                        case DataModel.SystemType.Char:
                        case DataModel.SystemType.NChar:
                            _emit(" COLLATE SQL_Latin1_General_CP1_CS_AS");
                            break;
                        }
                    _emit(") OR (");
                        _emit("n.");
                        _emit(columnName);
                        _emit(" IS NULL AND ");
                        _emit("c.");
                        _emit(columnName);
                        _emit(" IS NOT NULL");
                    _emit(") OR (");
                        _emit("n.");
                        _emit(columnName);
                        _emit(" IS NOT NULL AND ");
                        _emit("c.");
                        _emit(columnName);
                        _emit(" IS NULL");
                    _emit(")");
                    _emitNewline();
                }
            }
            private             void                        _update(int indent)
            {
                _emitSpace(indent + 25);
                _emit("THEN N'UPDATE ");
                _emit(_tableFullname);

                bool    first = true;

                foreach(int columnIndex in _columnIndexes) {
                    var column = _columnList[columnIndex];

                    if (first) {
                        _emit("' +");
                        _emitNewline();
                        _emitSpace(indent + 32);
                        _emit("N' SET ");
                        first = false;
                    }
                    else {
                        _emit(",' +");
                        _emitNewline();
                        _emitSpace(indent + 36);
                        _emit("N' ");
                    }

                    _emit(Library.SqlStatic.QuoteName(column.Name));
                    _emit("=' + ");
                    _emit(_strNewValue("n.", column));
                    _emit(" + '");
                }

                _emit("' + ");
                _emitNewline();

                _emitSpace(indent + 30);
                _emit("N' WHERE ");
                _emit(_keyName);
                _emit("=' + ");
                _emit(_strNewValue("c.", _columnList[0]));
                _emitNewline();
            }
            private             void                        _delete(int indent)
            {
                _emitSpace(indent + 25);
                _emit("THEN N'DELETE FROM ");
                    _emit(_tableFullname);
                    _emit("' +");
                    _emitNewline();
                _emitSpace(indent + 35);
                _emit("N' WHERE ");
                    _emit(_keyName);
                    _emit("=' + ");
                    _emit(_strNewValue("c.", _columnList[0]));
                    _emitNewline();
            }
            private             int                         _valueDataSet(int indent)
            {
                var     count      = 0;
                var     firstRec   = true;

                foreach (var r in _values) {
                    if (r.Fields != null) {
                        if (firstRec) {
                            _emitSpace(indent + 10);
                            firstRec = false;
                        }
                        else {
                            _emitSpace(indent);
                            _emit("UNION ALL ");
                        }

                        _emit("SELECT ");

                        _appendColumnValue(_columnList[0], r.Value);

                        foreach(int columnIndex in _columnIndexes) {
                            var column = _columnList[columnIndex];
                            _emit(", ");

                            r.Fields.TryGetValue(column.Name, out var value);
                            _appendColumnValue(column, value?.Value);
                        }

                        _emitNewline();
                        ++count;
                    }
                }

                return count;
            }
            private static      string                      _strNewValue(string setName, DataModel.Column column)
            {
                string      columnName = setName + Library.SqlStatic.QuoteName(column.Name);

                switch(column.SqlType.NativeType.SystemType) {
                case DataModel.SystemType.Bit:
                case DataModel.SystemType.TinyInt:
                case DataModel.SystemType.SmallInt:
                case DataModel.SystemType.Int:
                case DataModel.SystemType.BigInt:
                case DataModel.SystemType.SmallMoney:
                case DataModel.SystemType.Money:
                case DataModel.SystemType.Numeric:
                case DataModel.SystemType.Decimal:
                case DataModel.SystemType.UniqueIdentifier:
                    return "ISNULL(CONVERT(NVARCHAR, " + columnName + "),N'NULL')";

                case DataModel.SystemType.Char:
                case DataModel.SystemType.VarChar:
                case DataModel.SystemType.NChar:
                case DataModel.SystemType.NVarChar:
                    return "ISNULL(N''''+REPLACE(" + columnName + ",'''','''''')+N'''',N'NULL')";

                case DataModel.SystemType.Real:
                case DataModel.SystemType.Float:
                    return "ISNULL(CONVERT(NVARCHAR, " + columnName + ",3),N'NULL')";

                case DataModel.SystemType.Date:
                case DataModel.SystemType.Time:
                case DataModel.SystemType.SmallDateTime:
                case DataModel.SystemType.DateTime:
                case DataModel.SystemType.DateTime2:
                case DataModel.SystemType.DateTimeOffset:
                    return "ISNULL(CONVERT(NVARCHAR, " + columnName + ",127),N'NULL')";

                //case DataModel.SystemType.Binary:
                //case DataModel.SystemType.VarBinary:
                default:
                    throw new InvalidOperationException("Con't know to to emit " + column.SqlType.NativeType.ToString() + ".");
                }
            }
            private             void                        _appendColumnValue(DataModel.Column column, object value)
            {
                _emit(Library.SqlStatic.QuoteName(column.Name));
                _emit("=");
                _emit(_strColumnValue(column, value));
            }
            private static      string                      _strColumnValue(DataModel.Column column, object value)
            {
                if (value == null)      return "NULL";
                if (value is string)    return Library.SqlStatic.QuoteString((string)value);
                if (value is int)       return ((int)value).ToString(System.Globalization.CultureInfo.InvariantCulture);
                if (value is decimal)   return ((decimal)value).ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
                if (value is double)    return ((int)value).ToString(System.Globalization.NumberFormatInfo.InvariantInfo);

                throw new InvalidOperationException("Con't know to to emit " + value.GetType().FullName + ".");
            }
            private             void                        _emit(string s)
            {
                _emitString.Append(s);
            }
            private             void                        _emitLine(string s)
            {
                _emit(s);
                _emitNewline();
            }
            private             void                        _emitSpace(int c)
            {
                _emitString.Append(' ', c);
            }
            private             void                        _emitNewline()
            {
                _emitString.Append("\n");
            }
        }
    }
}
