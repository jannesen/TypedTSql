using System;
using System.Collections.Generic;
using Jannesen.Language.TypedTSql.Logic;

namespace Jannesen.Language.TypedTSql.Node
{
    public class TypeDeclaration_Table: TypeDeclarationWithGrant
    {
        public      override    DataModel.SymbolType            EntityType      { get { return DataModel.SymbolType.TypeTable;  } }
        public      override    DataModel.EntityType            Entity          { get { return _entity;                         } }

        public      readonly    Table                           n_Table;

        private                 DataModel.EntityTypeTable       _entity;

        public                                                  TypeDeclaration_Table(Core.ParserReader reader)
        {
            ParseToken(reader, "AS");
            ParseToken(reader, "TABLE");
            n_Table = AddChild(new Table(reader, TableType.Variable));
            ParseGrant(reader);
        }

        public      override    void                            TranspileInit(Declaration_TYPE declaration, GlobalCatalog catalog, SourceFile sourceFile)
        {
            if ((_entity = catalog.DefineTypeTable(declaration.EntityName)) == null)
                throw new TranspileException(declaration.n_Name, "Duplicate definition of type.");

            _entity.TranspileInit(new DataModel.DocumentSpan(sourceFile.Filename, declaration));
        }
        public      override    void                            TranspileNode(Transpile.Context context)
        {
            n_Table.TranspileNode(context);
            TranspileGrant(context);
        }
        public      override    void                            Transpiled()
        {
            _entity.Transpiled(n_Table.Columns, n_Table.Indexes);
        }

        public      override    void                            Emit(Core.EmitWriter emitWriter, Declaration_TYPE type)
        {
            emitWriter.WriteText("DECLARE @table_object_id INT = (SELECT [type_table_object_id] FROM sys.table_types WHERE [schema_id] = SCHEMA_ID(");
                emitWriter.WriteText(Library.SqlStatic.QuoteString(type.n_Name.n_EntitiyName.Schema));
                emitWriter.WriteText(") AND [name]=");
                emitWriter.WriteText(Library.SqlStatic.QuoteString(type.n_Name.n_EntitiyName.Name));
                emitWriter.WriteText(" AND [is_table_type]=1);\r\n");

            emitWriter.WriteText("IF @table_object_id IS NOT NULL\r\n");

            emitWriter.WriteText("BEGIN\r\n");
                emitWriter.WriteText("    IF (SELECT COUNT(*) FROM sys.all_columns WHERE [object_id]=@table_object_id)<>");
                    emitWriter.WriteText(n_Table.n_Columns.Length.ToString());
                    emitWriter.WriteText("\r\n");

                int column_id = 1;

                foreach(var tableColumn in n_Table.n_Columns) {
                    emitWriter.WriteText("    OR NOT EXISTS (SELECT * FROM sys.all_columns WHERE [object_id]=@table_object_id AND [column_id]=");
                        emitWriter.WriteText(column_id.ToString());

                        emitWriter.WriteText(" AND [name]=");
                        emitWriter.WriteText(Library.SqlStatic.QuoteString(tableColumn.n_Name.ValueString));

                    if (tableColumn is Table_ColumnData) {
                        var dataColumn    = tableColumn.Column;
                        var columnSqlType = dataColumn.SqlType;

                        if (columnSqlType is DataModel.SqlTypeNative nativeType) {
                            emitWriter.WriteText(" AND [system_type_id]=");
                            emitWriter.WriteText(nativeType.SystemTypeId.ToString());

                            switch(nativeType.SystemType) {
                            case DataModel.SystemType.Binary:
                            case DataModel.SystemType.VarBinary:
                            case DataModel.SystemType.Char:
                            case DataModel.SystemType.NChar:
                            case DataModel.SystemType.VarChar:
                            case DataModel.SystemType.NVarChar:
                                emitWriter.WriteText(" AND [max_length]=");
                                emitWriter.WriteText(nativeType.MaxLength.ToString());
                                break;

                            case DataModel.SystemType.Float:
                                emitWriter.WriteText(" AND [precision]=");
                                emitWriter.WriteText(nativeType.Precision.ToString());
                                break;

                            case DataModel.SystemType.Decimal:
                            case DataModel.SystemType.Numeric:
                                emitWriter.WriteText(" AND [precision]=");
                                emitWriter.WriteText(nativeType.Precision.ToString());
                                emitWriter.WriteText(" AND [scale]=");
                                emitWriter.WriteText(nativeType.Scale.ToString());
                                break;
                            }
                        }

                        if (columnSqlType.Entity != null) {
                            emitWriter.WriteText(" AND [user_type_id]=(SELECT [user_type_id] FROM sys.types WHERE [schema_id]=SCHEMA_ID(");
                            emitWriter.WriteText(Library.SqlStatic.QuoteString(columnSqlType.Entity.EntityName.Schema));
                            emitWriter.WriteText(") AND [name]=");
                            emitWriter.WriteText(Library.SqlStatic.QuoteString(columnSqlType.Entity.EntityName.Name));
                            emitWriter.WriteText(")");
                        }

                        var columnFlags = tableColumn.Column.ValueFlags;
                        emitWriter.WriteText(" AND [is_nullable]=");
                        emitWriter.WriteText((columnFlags & DataModel.ValueFlags.Nullable) != 0 ? "1" : "0");
                        emitWriter.WriteText(" AND [is_rowguidcol]=");
                        emitWriter.WriteText((columnFlags & DataModel.ValueFlags.Rowguidcol) != 0 ? "1" : "0");
                        emitWriter.WriteText(" AND [is_identity]=");
                        emitWriter.WriteText((columnFlags & DataModel.ValueFlags.Identity) != 0 ? "1" : "0");

                        if (columnSqlType.NativeType.SystemType == DataModel.SystemType.Char ||
                            columnSqlType.NativeType.SystemType == DataModel.SystemType.NChar ||
                            columnSqlType.NativeType.SystemType == DataModel.SystemType.VarChar ||
                            columnSqlType.NativeType.SystemType == DataModel.SystemType.NVarChar)
                        {
                            emitWriter.WriteText(" AND [collation_name]=");
                            if (tableColumn.Column.CollationName != null)
                                emitWriter.WriteText(Library.SqlStatic.QuoteString(tableColumn.Column.CollationName));
                            else
                                emitWriter.WriteText("CONVERT(sysname,DATABASEPROPERTYEX(DB_NAME(),'Collation'))");
                        }
                    }

                    if (tableColumn is Table_ColumnComputed) {
                        emitWriter.WriteText(" AND [is_computed]=1");
                    }

                        emitWriter.WriteText(")\r\n");

                    ++column_id;
                }

                emitWriter.WriteText("        RAISERROR('Table type ");
                emitWriter.WriteText(type.n_Name.n_EntitiyName.Fullname.Replace("'", "''"));
                emitWriter.WriteText(" is invalid, please fix manual.', 16, 1);\r\n");

            emitWriter.WriteText("END\r\n");

            emitWriter.WriteText("ELSE\r\n");

            emitWriter.WriteText("BEGIN\r\n");
                emitWriter.WriteText("    CREATE TYPE ");
                    emitWriter.WriteText(type.n_Name.n_EntitiyName.Fullname);
                    emitWriter.WriteText("\r\n");
                    emitWriter.WriteText("    AS TABLE\r\n");
                    n_Table.Emit(emitWriter);
            emitWriter.WriteText(";\r\n");
            emitWriter.WriteText("END\r\n");
        }
    }
}
