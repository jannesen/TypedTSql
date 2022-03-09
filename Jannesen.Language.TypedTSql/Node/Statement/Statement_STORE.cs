using System;
using System.Collections.Generic;
using System.Text;
using Jannesen.Language.TypedTSql.Core;
using Jannesen.Language.TypedTSql.Library;

namespace Jannesen.Language.TypedTSql.Node
{
    //https://msdn.microsoft.com/en-US/library/ms175976.aspx
    //      BEGIN TRY
    //           { sql_statement | statement_block }
    //      END TRY
    //      BEGIN CATCH
    //           [ { sql_statement | statement_block } ]
    //      END CATCH
    [StatementParser(Core.TokenID.BEGIN, prio:2)]
    public class Statement_STORE: Statement
    {
        public class STORE_TARGET: Core.AstParseNode
        {
            public class SOURCE: Core.AstParseNode
            {
                public                      Query_Select_SELECT                 n_Source;

                public                                                          SOURCE(Core.ParserReader reader)
                {
                    ParseToken(reader, Core.TokenID.LrBracket);
                    AddChild(n_Source = new Query_Select_SELECT(reader, Query_SelectContext.StatementStore));
                    ParseToken(reader, Core.TokenID.RrBracket);

                }

                public      override        void                                TranspileNode(Transpile.Context context)
                {
                    n_Source.TranspileNode(new Transpile.ContextRowSets(context));
                }
            }
            public class OUTPUT: Core.AstParseNode
            {
                public      readonly    Node_AssignVariable                 n_Variable;
                public      readonly    IExprNode                           n_Column;

                public                                                      OUTPUT(Core.ParserReader reader)
                {
                    n_Variable = ParseVarVariable(reader);
                    ParseToken(reader, Core.TokenID.Equal);
                    n_Column = ParseExpression(reader);
                }
                public      override    void                                TranspileNode(Transpile.Context context)
                {
                    n_Column.TranspileNode(context);
                }
            }
            public class WITH: Core.AstParseNode
            {
                public      readonly    bool                                n_DenyInsert;
                public      readonly    bool                                n_DenyUpdate;
                public      readonly    bool                                n_DenyDelete;
                public      readonly    Core.TokenWithSymbol[]              n_DenyUpdateofColumn;

                public                                                      WITH(Core.ParserReader reader)
                {
                    ParseToken(reader);
                    ParseToken(reader, "DENY");

                    do {
                        switch(ParseToken(reader, "INSERT", "UPDATE", "DELETE", "UPDATEOF").Text.ToUpperInvariant()) {
                        case "INSERT":      n_DenyInsert = true;        break;
                        case "UPDATE":      n_DenyUpdate = true;        break;
                        case "DELETE":      n_DenyDelete = true;        break;
                        case "UPDATEOF": {
                                var columns = new List<Core.TokenWithSymbol>();

                                ParseToken(reader, TokenID.LrBracket);

                                do {
                                    columns.Add(ParseName(reader));
                                }
                                while (ParseOptionalToken(reader, TokenID.Comma) != null);

                                ParseToken(reader, TokenID.RrBracket);

                                n_DenyUpdateofColumn = columns.ToArray();
                            }
                            break;
                        }
                    }
                    while (ParseOptionalToken(reader, TokenID.Comma) != null);
                }
                public      override    void                                TranspileNode(Transpile.Context context)
                {
                    if (n_DenyUpdateofColumn != null) {
                        foreach(var denycolumn in n_DenyUpdateofColumn) {
                            var column = context.RowSets[0].Columns.FindColumn(denycolumn.ValueString, out var ambigous);

                            if (column != null) {
                                denycolumn.SetSymbolUsage(column, DataModel.SymbolUsageFlags.Reference);
                                context.CaseWarning(denycolumn, column.Name);
                            }
                            else
                                context.AddError(denycolumn, "Unknown column " + SqlStatic.QuoteName(denycolumn.ValueString) + ".");
                        }
                    }
                }

                public                  bool                                denyColumn(string name)
                {
                    foreach(var c in n_DenyUpdateofColumn) {
                        if (string.Compare(c.ValueString, name, StringComparison.OrdinalIgnoreCase) == 0)
                            return true;
                    }

                    return false;
                }
            }

            struct TranspiledData
            {
                public  string                  NamePrefix;
                public  string                  TargetFullname;
                public  List<ColumnInfo>        ColumnsInfo;
                public  ColumnInfo              TargetTestColumn;
                public  ColumnInfo              SourceTestColumn;
                public  bool                    SingleRecord;
                public  bool                    hasInsert;
                public  bool                    hasUpdate;
                public  bool                    hasDelete;
                public  bool                    hasOutput;
                public  string                  DefaultCollate;
            }
            class ColumnInfo
            {
                public  string                  SafeName;
                public  DataModel.Column        Column;
                public  IExprNode               WhereLinkExpr;
                public  bool                    Nullable;
                public  bool                    Identity;
                public  bool                    Computed;
                public  bool                    Source;
                public  bool                    SourceNullable;
                public  bool                    SourceCheckNullInsert;
                public  bool                    SourceCheckNullUpdate;
                public  bool                    Key;
                public  bool                    Link;
                public  bool                    WhereKey;
                public  bool                    Inserted;
                public  bool                    Updated;
                public  bool                    RecVersion;
                public  DataModel.Variable      Output;
                public  string                  EmitWhereExpr;
            }
            class WhereLinkColumn
            {
                public  DataModel.Column        Column;
                public  IExprNode               Expr;
            }
            class DeclareColumn
            {
                public  string                  Name;
                public  string                  Type;
            }

            public      readonly    Node_EntityNameReference            n_Target;
            public      readonly    SOURCE                              n_Source;
            public      readonly    IExprNode                           n_Where;
            public      readonly    OUTPUT[]                            n_Outputs;
            public      readonly    WITH                                n_With;

            private                 TranspiledData                      _TD;

            public                                                      STORE_TARGET(Core.ParserReader reader)
            {
                ParseToken(reader, "TARGET");
                AddChild(n_Target = new Node_EntityNameReference(reader, EntityReferenceType.Table, DataModel.SymbolUsageFlags.Select));

                ParseToken(reader, "SOURCE");
                AddChild(n_Source = new SOURCE(reader));

                ParseToken(reader, "WHERE");
                AddChild(n_Where = ParseExpression(reader));

                if (ParseOptionalToken(reader, "OUTPUT") != null) {
                    var output = new List<OUTPUT>();

                    do {
                        output.Add(AddChild(new OUTPUT(reader)));
                    }
                    while (ParseOptionalToken(reader, TokenID.Comma) != null);

                    n_Outputs = output.ToArray();
                }

                if (reader.CurrentToken.isToken(TokenID.WITH)) {
                    n_With = AddChild(new WITH(reader));
                }
            }

            public      override    void                                TranspileNode(Transpile.Context context)
            {
                n_Target.TranspileNode(context);
                var targetContext = new Transpile.ContextStatementQuery(context);
                targetContext.SetTarget(n_Target);
                n_Source.TranspileNode(targetContext);

                var targetTable = (DataModel.ITable)n_Target.Entity;
                if (targetTable != null) {
                    var contextRowSet    = new Transpile.ContextRowSets(context);
                    contextRowSet.RowSets.Add(new DataModel.RowSet("", targetTable.Columns, source: n_Target.Entity));

                    n_Where.TranspileNode(contextRowSet);
                    n_Outputs?.TranspileNodes(contextRowSet);
                    n_With?.TranspileNode(contextRowSet);

                    _TD  = new TranspiledData();

                    try {
                        var t = new TranspileHelper(context);

                        if (t.Transpile(this)) {
                            _TD = t.TD;
                        }
                    }
                    catch(Exception err) {
                        context.AddError(this, err);
                    }
                }

                var usage = DataModel.SymbolUsageFlags.Select | DataModel.SymbolUsageFlags.Insert | DataModel.SymbolUsageFlags.Update | DataModel.SymbolUsageFlags.Delete;
                if (n_With != null) {
                    if (n_With.n_DenyInsert) usage &=~DataModel.SymbolUsageFlags.Insert;
                    if (n_With.n_DenyUpdate) usage &=~DataModel.SymbolUsageFlags.Update;
                    if (n_With.n_DenyDelete) usage &=~DataModel.SymbolUsageFlags.Delete;
                }
                n_Target.SetUsage(usage);
            }
            public                  void                                EmitPre(EmitContext emitContext)
            {
                foreach(var columninfo in _TD.ColumnsInfo) {
                    if (columninfo.WhereLinkExpr != null) {
                        var emitString = new Core.EmitWriterString(emitContext);
                        columninfo.WhereLinkExpr.Emit(new Core.EmitWriterTrimFull(emitString));
                        columninfo.EmitWhereExpr = (columninfo.WhereLinkExpr.NoBracketsNeeded) ? emitString.String : "(" + emitString.String + ")";
                    }
                }
            }
            public                  void                                EmitDeclare(EmitWriter emitWriter, int indent)
            {
                var declareColumns = new List<DeclareColumn>();

                declareColumns.Add(new DeclareColumn() { Name="$action", Type="VARCHAR(150)" });

                foreach(var c in _TD.ColumnsInfo) {
                    if (c.WhereKey)
                        declareColumns.Add(new DeclareColumn() { Name="K" + c.SafeName, Type=c.Column.SqlType.ToSql() });
                }
                foreach(var c in _TD.ColumnsInfo) {
                    if (c.Inserted || c.Updated)
                        declareColumns.Add(new DeclareColumn() { Name="N" + c.SafeName, Type=c.Column.SqlType.ToSql() });
                }

                int maxlength_Name = 0;
                int maxlength_Type = 0;

                foreach (var c in declareColumns) {
                    if (maxlength_Name < c.Name.Length) maxlength_Name = c.Name.Length;
                    if (maxlength_Type < c.Type.Length) maxlength_Type = c.Type.Length;
                }

                maxlength_Name = maxlength_Name + 8 - maxlength_Name % 4;
                maxlength_Type = maxlength_Type + 8 - maxlength_Type % 4;

                emitWriter.WriteNewLine(-1);

                if (_TD.SingleRecord) {
                    for (int i = 0 ; i < declareColumns.Count ; ++i) {
                        var c = declareColumns[i];

                        if (i == 0) {
                            emitWriter.WriteNewLine(indent, "DECLARE ");
                        }
                        else
                            emitWriter.WriteNewLine(indent + 8);

                        emitWriter.WriteText(_TD.NamePrefix, c.Name);
                        emitWriter.WriteSpace(maxlength_Name - c.Name.Length);
                        emitWriter.WriteText(c.Type, (i < declareColumns.Count-1) ? "," : ";");
                    }
                }
                else {
                    emitWriter.WriteNewLine(indent, "DECLARE ", _TD.NamePrefix, " TABLE (");

                    for (int i = 0 ; i < declareColumns.Count ; ++i) {
                        var c = declareColumns[i];
                        emitWriter.WriteNewLine(indent + 12, SqlStatic.QuoteName(c.Name));

                        emitWriter.WriteSpace(maxlength_Name - c.Name.Length);
                        emitWriter.WriteText(c.Type);
                        emitWriter.WriteSpace(maxlength_Type - c.Type.Length);
                        emitWriter.WriteText("NULL");

                        if (i < declareColumns.Count-1)
                            emitWriter.WriteText(",");
                    }

                    emitWriter.WriteNewLine(indent + 8, ");");
                }
            }
            public                  void                                EmitCompare(EmitWriter emitWriter, int indent, string errtmpvar)
            {
                emitWriter.WriteNewLine(-1);
                _emitCompare_select(emitWriter, indent);
                _emitCompare_validate(emitWriter, indent, errtmpvar);
            }
            public                  void                                EmitDelete(EmitWriter emitWriter, int indent)
            {
                if (_TD.hasDelete) {
                    _emit_if_action(emitWriter, indent, "D");

                    if (_TD.SingleRecord) {
                        emitWriter.WriteNewLine(indent + 4, "DELETE FROM ", _TD.TargetFullname);
                        _emit_single_where(emitWriter, indent + 10);
                        emitWriter.WriteText(";");
                        _emit_set_output(emitWriter, indent + 4, 'D');
                    }
                    else {
                        emitWriter.WriteNewLine(indent + 4, "DELETE FROM [$target]");
                        _emit_multy_from(emitWriter, indent + 11);
                        emitWriter.WriteNewLine(indent + 10, "WHERE [$t].[$action] = 'D'");
                        emitWriter.WriteNewLine(indent + 9, "OPTION (FORCE ORDER)");
                        emitWriter.WriteText(";");
                    }

                    emitWriter.WriteNewLine(indent, "END");
                }
            }
            public                  void                                EmitUpdate(EmitWriter emitWriter, int indent)
            {
                if (_TD.hasUpdate) {
                    _emit_if_action(emitWriter, indent, "U");

                    if (_TD.SingleRecord) {
                        emitWriter.WriteNewLine(indent + 4, "UPDATE ", _TD.TargetFullname);
                        _emit_updateset(emitWriter, indent + 7);
                        _emit_single_where(emitWriter, indent + 5);
                        emitWriter.WriteText(";");
                        _emit_set_output(emitWriter, indent + 4, 'U');
                    }
                    else {
                        emitWriter.WriteNewLine(indent + 4, "UPDATE [$target]");
                        _emit_updateset(emitWriter, indent + 7);
                        _emit_multy_from(emitWriter, indent + 6);
                        emitWriter.WriteNewLine(indent + 5, "WHERE [$t].[$action] = 'U'");
                        emitWriter.WriteNewLine(indent + 4, "OPTION (FORCE ORDER)");
                        emitWriter.WriteText(";");
                    }

                    emitWriter.WriteNewLine(indent, "END");
                }
            }
            public                  void                                EmitInsert(EmitWriter emitWriter, int indent)
            {
                if (_TD.hasInsert) {
                    bool next;
                    _emit_if_action(emitWriter, indent, "I");

                    emitWriter.WriteNewLine(indent + 4, "INSERT INTO ", _TD.TargetFullname, "(");

                    next = false;
                    foreach (var c in _TD.ColumnsInfo) {
                        if (c.Inserted || (c.WhereLinkExpr != null && !c.Identity && !c.Computed) || c.RecVersion) {
                            if (next)
                                emitWriter.WriteText(", ");

                            emitWriter.WriteText(SqlStatic.QuoteName(c.Column.Name));
                            next = true;
                        }
                    }
                    emitWriter.WriteText(")");

                    emitWriter.WriteNewLine(indent + 4, "SELECT ");
                    next = false;
                    foreach (var c in _TD.ColumnsInfo) {
                        if (c.Inserted || (c.WhereLinkExpr != null && !c.Identity) || c.RecVersion) {
                            if (next)
                                emitWriter.WriteText(", ");

                            if (c.RecVersion)
                                emitWriter.WriteText("0");
                            else if (c.Inserted)
                                emitWriter.WriteText(_TD.SingleRecord ? _TD.NamePrefix + "N" + c.SafeName : SqlStatic.QuoteName("N" + c.SafeName));
                            else
                                c.WhereLinkExpr.Emit(new Core.EmitWriterTrimFull(emitWriter));

                            next = true;
                        }
                    }

                    if (!_TD.SingleRecord) {
                        emitWriter.WriteNewLine(indent + 6, "FROM ", _TD.NamePrefix);
                        emitWriter.WriteNewLine(indent + 5, "WHERE [$action] = 'I'");
                    }

                    emitWriter.WriteText(";");
                    _emit_set_output(emitWriter, indent + 4, 'I');
                    emitWriter.WriteNewLine(indent, "END");
                }
            }

            private                 void                                _emitCompare_select(EmitWriter emitWriter, int indent)
            {
                if (!_TD.SingleRecord) {
                    emitWriter.WriteNewLine(indent, "INSERT INTO ", _TD.NamePrefix, "([$action]");

                    foreach(var c in _TD.ColumnsInfo) {
                        if (c.WhereKey)
                            emitWriter.WriteText(", ", SqlStatic.QuoteName("K" + c.SafeName));
                    }
                    foreach(var c in _TD.ColumnsInfo) {
                        if (c.Inserted || c.Updated)
                            emitWriter.WriteText(", ", SqlStatic.QuoteName("N" + c.SafeName));
                    }

                    emitWriter.WriteText(")");

                    emitWriter.WriteNewLine(indent, "SELECT [$action] = ");
                    _emitCompare_selectAction(emitWriter, indent + 19);
                }
                else {
                    emitWriter.WriteNewLine(indent, "SELECT ", _TD.NamePrefix , "$action = ");
                    _emitCompare_selectAction(emitWriter, indent + 17 + _TD.NamePrefix.Length);
                }

                _emitCompare_selectColumms(emitWriter, indent + 7);
                emitWriter.WriteNewLine(indent + 2, "FROM (");
                _emitCompare_target(emitWriter, indent + 11);
                emitWriter.WriteNewLine(indent + 7, ") [$target]");
                emitWriter.WriteNewLine(indent + 7, "FULL OUTER JOIN");
                _emitCompare_source(emitWriter, indent + 7);
                emitWriter.WriteText(" [$src] ON ");
                _emitCompare_on(emitWriter);
            }
            private                 void                                _emitCompare_selectAction(EmitWriter emitWriter, int indent)
            {
                bool next;

                emitWriter.WriteText("CASE ");

                // Check delete
                emitWriter.WriteText("WHEN [$src].", (_TD.SourceTestColumn != null ? SqlStatic.QuoteName(_TD.SourceTestColumn.Column.Name) : "[$record]"), " IS NULL");
                emitWriter.WriteNewLine(indent + 10, "THEN ", _TD.hasDelete ? "'D'" : "'Delete not allowed.'");

                // Check src null fields
                foreach(var c in _TD.ColumnsInfo) {
                    if (c.SourceCheckNullInsert) {
                        var s = SqlStatic.QuoteName(c.Column.Name);
                        emitWriter.WriteNewLine(indent +  5, "WHEN [$src].", s, " IS NULL");
                        emitWriter.WriteNewLine(indent + 10, "THEN " + SqlStatic.QuoteString(s + " is null in source."));
                    }
                }

                // Check insert
                emitWriter.WriteNewLine(indent +  5, "WHEN [$target].", (_TD.TargetTestColumn != null ? SqlStatic.QuoteName(_TD.TargetTestColumn.Column.Name) : "[$record]"), " IS NULL");
                if (_TD.SingleRecord) {
                    emitWriter.WriteNewLine(indent + 10, "THEN CASE WHEN ");
                    next = false;
                    foreach (var c in _TD.ColumnsInfo) {
                        if (c.WhereLinkExpr != null) {
                            if (next)
                                emitWriter.WriteNewLine(indent + 21, " AND ");

                            emitWriter.WriteText(c.EmitWhereExpr, " IS NULL");
                            next = true;
                        }
                    }
                    emitWriter.WriteNewLine(indent + 26, "THEN ", _TD.hasInsert ? "'I'" : "'Insert not allowed.'");
                    emitWriter.WriteNewLine(indent + 26, "ELSE 'Record no longer exists'");
                    emitWriter.WriteNewLine(indent + 15, "END");
                }
                else
                    emitWriter.WriteNewLine(indent + 10, "THEN ", _TD.hasInsert ? "'I'" : "'Insert not allowed.'");

                // Check deny update columns and recversion
                foreach(var c in _TD.ColumnsInfo) {
                    if (c.SourceCheckNullUpdate) {
                        var s = SqlStatic.QuoteName(c.Column.Name);
                        emitWriter.WriteNewLine(indent +  5, "WHEN [$src].", s, " IS NULL");
                        emitWriter.WriteNewLine(indent + 10, "THEN " + SqlStatic.QuoteString(s + " is null in source."));
                    }

                    if (c.Source && !c.Updated && !c.Link && !c.RecVersion) {
                        var s = SqlStatic.QuoteName(c.Column.Name);
                        emitWriter.WriteNewLine(indent + 5, "WHEN ", "([$target].", s, " <> ", "[$src].", s);

                        var collate = c.Column.CollationName;
                        if (collate != null)
                            emitWriter.WriteText(" COLLATE ", (collate == "database_default") ? _TD.DefaultCollate : _collateSensitive(collate));

                        if (c.Nullable)
                            emitWriter.WriteText(" OR ([$target].", s, " IS NOT NULL AND [$src].", s, " IS NULL)  OR ([$target].", s, " IS NULL AND [$src].", s, " IS NOT NULL)");
                        emitWriter.WriteText(")");

                        emitWriter.WriteNewLine(indent + 10, "THEN " + SqlStatic.QuoteString("Update of " + s + " not allowed."));
                    }
                }

                // Check record version
                foreach(var c in _TD.ColumnsInfo) {
                    if (c.RecVersion) {
                        var s = SqlStatic.QuoteName(c.Column.Name);
                        emitWriter.WriteNewLine(indent + 5, "WHEN ", "[$target].", s, " <> ", "[$src].", s);
                        emitWriter.WriteNewLine(indent + 10, "THEN " + SqlStatic.QuoteString("Data changed."));
                    }
                }

                // Check no action
                next = false;
                foreach(var c in _TD.ColumnsInfo) {
                    if (c.Updated) {
                        var s = SqlStatic.QuoteName(c.Column.Name);
                        emitWriter.WriteNewLine(indent + 5, next ? " AND " : "WHEN ", "([$target].", s, " = ", "[$src].", s);

                        var collate = c.Column.CollationName;
                        if (collate != null)
                            emitWriter.WriteText(" COLLATE ", (collate == "database_default") ? _TD.DefaultCollate : _collateSensitive(collate));

                        if (c.Nullable)
                            emitWriter.WriteText(" OR ([$target].", s, " IS NULL AND [$src].", s, " IS NULL)");

                        emitWriter.WriteText(")");
                        next = true;
                    }
                }

                if (next) {
                    emitWriter.WriteNewLine(indent + 10, "THEN '-'");
                    emitWriter.WriteNewLine(indent + 10, "ELSE ", _TD.hasUpdate ? "'U'" : "'Update not allowed.'");
                }
                else
                    emitWriter.WriteNewLine(indent + 10, "ELSE '-'");

                // done
                emitWriter.WriteNewLine(indent     , "END");
            }
            private                 void                                _emitCompare_selectColumms(EmitWriter emitWriter, int indent)
            {
                int namelength = 8;

                foreach(var c in _TD.ColumnsInfo) {
                    if ((c.Key && c.WhereLinkExpr == null) || (c.Inserted || c.Updated)) {
                        var n = c.SafeName;
                        if (namelength < n.Length)
                            namelength = n.Length;
                    }
                }
                namelength = namelength + 2 + (_TD.SingleRecord ? _TD.NamePrefix.Length : 2);

                foreach(var c in _TD.ColumnsInfo) {
                    if (c.Output != null) {
                        var n = c.Output.SqlName ?? c.Output.Name;
                        if (namelength < n.Length)
                            namelength = n.Length;
                    }
                }

                foreach(var c in _TD.ColumnsInfo) {
                    if (c.WhereKey) {
                        var tname = _TD.SingleRecord ? _TD.NamePrefix + "K" + c.SafeName : SqlStatic.QuoteName("K" + c.SafeName);
                        emitWriter.WriteText(",");
                        emitWriter.WriteNewLine(indent, tname);
                        emitWriter.WriteSpace(namelength - tname.Length);
                        emitWriter.WriteText("= [$target].", SqlStatic.QuoteName(c.Column.Name));
                    }
                }

                foreach(var c in _TD.ColumnsInfo) {
                    if (c.Inserted || c.Updated) {
                        var tname = _TD.SingleRecord ? _TD.NamePrefix + "N" + c.SafeName : SqlStatic.QuoteName("N" + c.SafeName);
                        emitWriter.WriteText(",");
                        emitWriter.WriteNewLine(indent, tname);
                        emitWriter.WriteSpace(namelength - tname.Length);
                        emitWriter.WriteText("= [$src].", SqlStatic.QuoteName(c.Column.Name));
                    }
                }

                if (_TD.SingleRecord) {
                    foreach(var c in _TD.ColumnsInfo) {
                        if (c.Output != null) {
                            var tname = c.Output.SqlName ?? c.Output.Name;
                            emitWriter.WriteText(",");
                            emitWriter.WriteNewLine(indent, tname);
                            emitWriter.WriteSpace(namelength - tname.Length);
                            emitWriter.WriteText("= [$target].", SqlStatic.QuoteName(c.Column.Name));
                        }
                    }
                }
            }
            private                 void                                _emitCompare_validate(EmitWriter emitWriter, int indent, string errtmpvar)
            {
                if (_TD.SingleRecord) {
                    emitWriter.WriteNewLine(-1);
                    emitWriter.WriteNewLine(indent, "IF @@ROWCOUNT < 1");
                    emitWriter.WriteNewLine(indent + 4, "THROW 50000,'Nothing to store.',0;");
                    emitWriter.WriteNewLine(-1);
                    emitWriter.WriteNewLine(indent, "IF @@ROWCOUNT > 1");
                    emitWriter.WriteNewLine(indent + 4, "THROW 50000,'Multiple record is store source while expecting one.',0;");
                    emitWriter.WriteNewLine(-1);
                    emitWriter.WriteNewLine(indent, "IF ", _TD.NamePrefix,"$action", " NOT IN ('-','I','U','D')");
                    emitWriter.WriteNewLine(indent + 4, "THROW 50000,", _TD.NamePrefix,"$action", ",0;");
                }
                else {
                    emitWriter.WriteNewLine(-1);
                    emitWriter.WriteNewLine(indent, "SELECT ", errtmpvar, " = (SELECT TOP(1) [$action] FROM ", _TD.NamePrefix, " WHERE [$action] NOT IN ('-','I','U','D'));");
                    emitWriter.WriteNewLine(indent, "IF ", errtmpvar, " IS NOT NULL");
                    emitWriter.WriteNewLine(indent + 4, "THROW 50000,", errtmpvar, ",0;");

                    _emitCompare_validate_dubkey(emitWriter, indent, errtmpvar, true);
                    _emitCompare_validate_dubkey(emitWriter, indent, errtmpvar, false);
                }
            }
            private                 void                                _emitCompare_validate_dubkey(EmitWriter emitWriter, int indent, string errtmpvar, bool ks)
            {
                var keys = new List<string>();

                foreach (var c in _TD.ColumnsInfo) {
                    if (c.Key) {
                        if (ks) {
                            if (c.WhereLinkExpr == null)
                                keys.Add("K" + c.SafeName);
                        }
                        else {
                            if (c.Inserted || c.Updated)
                                keys.Add("N" + c.SafeName);
                        }
                    }
                }

                if (keys.Count > 0) {
                    emitWriter.WriteNewLine(-1);
                    emitWriter.WriteNewLine(indent, "IF EXISTS (SELECT 1 FROM ", _TD.NamePrefix);
                    for (int i = 0 ; i < keys.Count ; ++i)
                        emitWriter.WriteText(i == 0 ? " WHERE " : " AND ", SqlStatic.QuoteName(keys[i]), " IS NOT NULL");
                    for (int i = 0 ; i < keys.Count ; ++i)
                        emitWriter.WriteText(i == 0 ? " GROUP BY " : ", ", SqlStatic.QuoteName(keys[i]));

                    emitWriter.WriteText(" HAVING COUNT(*) > 1)");
                    emitWriter.WriteNewLine(indent + 4, "THROW 50000,'Duplicate record in store source.',0;");
                }
            }
            private                 void                                _emitCompare_target(EmitWriter emitWriter, int indent)
            {
                emitWriter.WriteNewLine(indent, "SELECT *");
                emitWriter.WriteNewLine(indent, "  FROM ");
                emitWriter.WriteText(_TD.TargetFullname, " WITH(HOLDLOCK,UPDLOCK)");
                emitWriter.WriteNewLine(indent, " WHERE ");
                n_Where.Emit(new Core.EmitWriterTrimBeginEnd(emitWriter));
            }
            private                 void                                _emitCompare_source(EmitWriter emitWriter, int indent)
            {
                if (_TD.SourceTestColumn == null) {
                    emitWriter.WriteNewLine(indent    , "(");
                    emitWriter.WriteNewLine(indent + 4, "SELECT *, [$record]=1");
                    emitWriter.WriteNewLine(indent + 6, "FROM ");
                    n_Source.Emit(new Core.EmitWriterTrimBeginEnd(emitWriter));
                    emitWriter.WriteText(" [$dummy]");
                    emitWriter.WriteNewLine(indent, ")");
                }
                else {
                    emitWriter.WriteNewLine(indent);
                    n_Source.Emit(new Core.EmitWriterTrimBeginEnd(emitWriter));
                }
            }
            private                 void                                _emitCompare_on(EmitWriter emitWriter)
            {
                var i = emitWriter.Linepos;

                if (_TD.SingleRecord) {
                    emitWriter.WriteText("1=1");
                }
                else {
                    bool    first = true;

                    foreach (var c in _TD.ColumnsInfo) {
                        if (c.Link) {
                            if (!first)
                                emitWriter.WriteNewLine(i - 4, "AND ");

                            var s = SqlStatic.QuoteName(c.Column.Name);
                            _testEqual(emitWriter, "[$src]." + s, "[$target]." + s, c.Nullable);
                            first = false;
                        }
                    }
                }
            }

            private                 void                                _emit_if_action(EmitWriter emitWriter, int indent, string action)
            {
                emitWriter.WriteNewLine(-1);

                if (_TD.SingleRecord)
                    emitWriter.WriteNewLine(indent, "IF ", _TD.NamePrefix, "$action", " = '" + action + "'");
                 else
                    emitWriter.WriteNewLine(indent, "IF EXISTS (SELECT * FROM ", _TD.NamePrefix, " WHERE [$action] = '" + action + "')");

                emitWriter.WriteNewLine(indent, "BEGIN");
            }
            private                 void                                _emit_updateset(EmitWriter emitWriter, int indent)
            {
                int     maxlength = 0;
                bool    first     = true;

                foreach(var c in _TD.ColumnsInfo) {
                    if (c.Updated || c.RecVersion) {
                        if (maxlength < c.Column.Name.Length)
                            maxlength = c.Column.Name.Length;
                    }
                }

                foreach(var c in _TD.ColumnsInfo) {
                    if (c.Updated || c.RecVersion) {
                        if (first) {
                            emitWriter.WriteNewLine(indent, "SET ");
                        }
                        else {
                            emitWriter.WriteText(",");
                            emitWriter.WriteNewLine(indent + 4);
                        }

                        if (!_TD.SingleRecord)
                            emitWriter.WriteText("[$target].");

                        emitWriter.WriteText(SqlStatic.QuoteName(c.Column.Name));
                        emitWriter.WriteSpace(maxlength - c.Column.Name.Length);
                        emitWriter.WriteText(" = ");

                        if (c.RecVersion)
                            emitWriter.WriteText(SqlStatic.QuoteName(c.Column.Name), " + 1");
                        else if (_TD.SingleRecord)
                            emitWriter.WriteText(_TD.NamePrefix, "N", c.SafeName);
                        else
                            emitWriter.WriteText("[$t].", SqlStatic.QuoteName("N" + c.SafeName));

                        first = false;
                    }
                }
            }
            private                 void                                _emit_single_where(EmitWriter emitWriter, int indent)
            {
                bool first = true;
                foreach (var c in _TD.ColumnsInfo) {
                    if (c.Key) {
                        emitWriter.WriteNewLine(indent, first ? "WHERE " : "  AND ");
                        _testEqual(emitWriter,
                                   SqlStatic.QuoteName(c.Column.Name),
                                   c.WhereKey ? _TD.NamePrefix + "K" + c.SafeName : c.EmitWhereExpr,
                                   c.Nullable);
                        first = false;
                    }
                }
            }
            private                 void                                _emit_multy_from(EmitWriter emitWriter, int indent)
            {
                emitWriter.WriteNewLine(indent, "FROM ", _TD.NamePrefix, " [$t]");
                emitWriter.WriteNewLine(indent + 5, "INNER LOOP JOIN ", _TD.TargetFullname, " [$target]");

                var i = emitWriter.Linepos;
                var first = true;

                foreach (var c in _TD.ColumnsInfo) {
                    if (c.Key) {
                        if (first)
                            emitWriter.WriteText(" ON ");
                        else
                            emitWriter.WriteNewLine(i, "AND ");

                        _testEqual(emitWriter,
                                   "[$target]." + SqlStatic.QuoteName(c.Column.Name),
                                   c.WhereKey ? "[$t]." + SqlStatic.QuoteName("K" + c.SafeName) : c.EmitWhereExpr,
                                   c.Nullable);
                        first = false;
                    }
                }

            }
            private                 void                                _emit_set_output(EmitWriter emitWriter, int indent, char action)
            {
                foreach(var c in _TD.ColumnsInfo) {
                    if (c.Output != null) {
                        if (action == 'U' && c.Identity)
                            continue;

                        emitWriter.WriteNewLine(indent, "SET ", c.Output.SqlName ?? c.Output.Name, " = ");

                        if (action == 'D') {
                            emitWriter.WriteText("NULL;");
                        }
                        else if (action == 'U' || action == 'I')
                        {
                            if (c.Identity)
                                emitWriter.WriteText("SCOPE_IDENTITY();");
                            else
                                emitWriter.WriteText(_TD.NamePrefix, "N", c.SafeName, ";");
                        }
                    }
                }
            }
            private     static      void                                _testEqual(EmitWriter emitWriter, string v1, string v2, bool nullable)
            {
                if (nullable)
                    emitWriter.WriteText("(", v1, " = ", v2, " OR (", v1, " IS NULL AND ", v2, " IS NULL))");
                else
                    emitWriter.WriteText(v1, " = ", v2);
            }

            private     static      string                              _collateSensitive(string s)
            {
                return s.Replace("_CI", "_CS").Replace("_AI", "_AS");
            }

            class TranspileHelper
            {
                public                  Transpile.Context                   context;
                public                  DataModel.ITable                    targetTable;
                public                  STORE_TARGET                        Target;
                public                  TranspiledData                      TD;

                public                                                      TranspileHelper(Transpile.Context context)
                {
                    this.context = context;
                }

                public                  bool                                Transpile(STORE_TARGET target)
                {
                    Target      = target;
                    targetTable = (DataModel.ITable)target.n_Target.Entity;
                    TD          = new TranspiledData() {
                                      NamePrefix     = "@ST" + (++context.RootContext.StoreTargetNr).ToString(System.Globalization.CultureInfo.InvariantCulture) + "$",
                                      TargetFullname = (targetTable is DataModel.EntityObjectTable table ? table.EntityName.Fullname : ((DataModel.ISymbol)targetTable).Name),
                                      DefaultCollate = _collateSensitive(context.Catalog.DefaultCollation)
                                  };

                    if (!_initColumns())                return false;
                    if (!_source(target.n_Source))      return false;
                    if (!_where(target.n_Where))        return false;
                    if (!_process())                    return false;
                    if (!_output(target.n_Outputs))     return false;

                    _filterColumns();

                    return true;
                }

                private                 bool                                _initColumns()
                {
                    TD.ColumnsInfo = new List<ColumnInfo>(targetTable.Columns.Count);

                    var columns = targetTable.Columns;

                    for (int i = 0 ; i < columns.Count ; ++i) {
                        var column   = columns[i];
                        var saveName = new StringBuilder(column.Name.Length + 6);

                        saveName.Append(i);
                        saveName.Append('$');

                        for (int j = 0 ; j < column.Name.Length && j < 64 ; ++j) {
                            char chr = column.Name[j];
                            if (('0' <= chr && chr <= '9') ||
                                ('A' <= chr && chr <= 'Z') ||
                                ('a' <= chr && chr <= 'z') ||
                                (chr == '_'))
                                saveName.Append(chr);
                            else
                                saveName.Append('_');
                        }

                        TD.ColumnsInfo.Add(new ColumnInfo() {
                                                SafeName   = saveName.ToString(),
                                                Column     = column,
                                                Nullable   = column.isNullable,
                                                Identity   = column.isIdentity,
                                                Computed   = column.isComputed,
                                                RecVersion = (column.SqlType.TypeFlags & DataModel.SqlTypeFlags.RecVersion) != 0});
                    }

                    var primarykey = _findKey();

                    if (primarykey == null) {
                        context.AddError(Target.n_Target, "Target table has no primary-key.");
                        return false;
                    }

                    foreach(var indexcolumn in primarykey.Columns)
                        _findColumn(indexcolumn.Column).Key = true;

                    return true;
                }
                private                 bool                                _source(SOURCE source)
                {
                    bool    rtn = true;
                    foreach(Query_Select_ColumnExpression sourceColumn in source.n_Source.n_Columns.n_Columns) {
                        var columninfo = _findColumn(sourceColumn.ResultColumn);
                        columninfo.Source = true;

                        if ((sourceColumn.n_Expression.ValueFlags & DataModel.ValueFlags.Nullable) != 0) {
                            columninfo.SourceNullable = true;

                            if (columninfo.Identity || columninfo.RecVersion)
                                columninfo.SourceCheckNullUpdate = true;
                            else
                            if (!(columninfo.Column.isNullable))
                                columninfo.SourceCheckNullInsert = true;
                        }
                    }

                    return rtn;
                }
                private                 bool                                _where(IExprNode where)
                {
                    bool    rtn = true;

                    var whereLinkedColumns = _where_Linked(where);
                    if (whereLinkedColumns != null) {
                        foreach(var x in whereLinkedColumns) {
                            var c = _findColumn(x.Column);

                            if (c.WhereLinkExpr != null) {
                                context.AddError(where, "Column " + SqlStatic.QuoteName(x.Column.Name) + " already linked in where.");
                                rtn = false;
                            }

                            c.WhereLinkExpr = x.Expr;
                        }
                    }

                    return rtn;
                }
                private                 WhereLinkColumn[]                   _where_Linked(IExprNode expr)
                {
                    if (expr is Expr_Operator_AndOr andor) {
                        if (andor.n_Operator.ID == TokenID.AND) {
                            var x1 = _where_Linked(andor.n_Expr1);
                            var x2 = _where_Linked(andor.n_Expr2);

                            if (x1 != null) {
                                if (x2 != null)
                                    return Library.Library.ArrayJoin(x1, x2);

                                return x1;
                            }

                            return x2;
                        }
                    }
                    else if (expr is Expr_SubExpr subexpr)
                    {
                        return _where_Linked(subexpr.n_Expr);
                    }
                    else if (expr is Expr_Operator_Compare compare)
                    {
                        if (compare.n_Operator.ID == TokenID.Equal) {
                            var x1 = _where_Link_ValueExpr(compare.n_Expr1);
                            var x2 = _where_Link_ValueExpr(compare.n_Expr2);

                            if (x2 is DataModel.Column && x1 is IExprNode) {
                                var t = x2;
                                x2 = x1;
                                x1 = t;
                                context.AddWarning(compare, "Please rewrite to <column> = <expr>");
                            }

                            if (x1 is DataModel.Column column && x2 is IExprNode expr2) {
                                if (column.isNullable)
                                    context.AddWarning(expr, "Column " + SqlStatic.QuoteName(column.Name) + " is nullable, using a '=' compare can have unexpected result. please use is_equal.");

                                return new WhereLinkColumn[] { new WhereLinkColumn() { Column=column, Expr=expr2 } };
                            }
                        }
                    }
                    else if (expr is BuildIn.Func.IS_EQUAL isequal)
                    {
                        if (isequal.n_Collate == null) {
                            var x1 = _where_Link_ValueExpr(isequal.n_Expr1);
                            var x2 = _where_Link_ValueExpr(isequal.n_Expr2);

                            if (x2 is DataModel.Column && x1 is IExprNode) {
                                var t = x2;
                                x2 = x1;
                                x1 = t;
                                context.AddWarning(isequal, "Please rewrite to is_equal(<column>, <expr>)");
                            }

                            if (x1 is DataModel.Column column1 && x2 is IExprNode expr2)
                                return new WhereLinkColumn[] { new WhereLinkColumn() { Column=column1, Expr=expr2 } };
                        }
                    }

                    return null;
                }
                private                 object                              _where_Link_ValueExpr(IExprNode expr)
                {
                    var column = expr.ReferencedColumn;
                    if (column != null && column.ParentSymbol == targetTable) {
                        return column;
                    }

                    return expr;
                }
                private                 bool                                _output(OUTPUT[] outputs)
                {
                    bool    rtn = true;

                    if (outputs != null) {
                        foreach(var output in outputs) {
                            output.n_Variable.TranspileAssign(context, output.n_Column);
                            var column = output.n_Column.ReferencedColumn;

                            if (column != null) {
                                var c = _findColumn(column);
                                c.Output = output.n_Variable.Variable;

                                if (!(c.Inserted || c.Updated || c.Identity)) {
                                    context.AddError(output.n_Column, "Output column " + SqlStatic.QuoteName(c.Column.Name) + " is not changed by STORE.");
                                    rtn = false;
                                }
                            }
                            else {
                                context.AddError(output.n_Column, "Expect a column name.");
                                rtn = false;
                            }
                        }
                    }

                    return rtn;
                }
                private                 bool                                _process()
                {
                    var         with           = Target.n_With;
                    bool        allowInsert    = with == null || !with.n_DenyInsert;
                    bool        allowUpdate    = with == null || !with.n_DenyUpdate;
                    bool        singleRecord   = true;
                    ColumnInfo  identityColumn = null;
                // test singleRecord
                // set ci.Updated
                // set ci.Inserted
                    foreach(var columninfo in TD.ColumnsInfo) {
                        if (columninfo.Key) {
                            if (!(columninfo.Source || columninfo.WhereLinkExpr != null)) {
                                context.AddError(Target.n_Where, "Missing primary-key column " + SqlStatic.QuoteName(columninfo.Column.Name));
                                return false;
                            }

                            if (columninfo.WhereLinkExpr == null)
                                singleRecord = false;

                            var sqlType = columninfo.Column.SqlType;
                            columninfo.WhereKey = !(columninfo.WhereLinkExpr != null && (sqlType.TypeFlags & DataModel.SqlTypeFlags.SimpleType) != 0 && sqlType.NativeType.isInteger);
                        }

                        if (columninfo.Source) {
                            if (!columninfo.Identity && !columninfo.RecVersion) {
                                if (allowUpdate && (with == null || !with.denyColumn(columninfo.Column.Name)))
                                    columninfo.Updated = true;

                                columninfo.Inserted = allowInsert;
                            }
                        }
                        else {
                            if (allowInsert && !columninfo.Computed && !columninfo.Nullable && columninfo.WhereLinkExpr == null && !columninfo.Identity && !columninfo.Column.hasDefault) {
                                context.AddError(Target, "Column " + Library.SqlStatic.QuoteName(columninfo.Column.Name) + " is not defined and can't be null.");
                                return false;
                            }

                            if (allowUpdate && columninfo.RecVersion) {
                                context.AddError(Target.n_Source, "Missing rec-version column " + SqlStatic.QuoteName(columninfo.Column.Name) + " in source.");
                                return false;
                            }
                        }

                        if (columninfo.Identity)
                            identityColumn = columninfo;

                        if (TD.TargetTestColumn == null && !columninfo.Nullable)
                            TD.TargetTestColumn = columninfo;

                        if (TD.SourceTestColumn == null && columninfo.Source && !columninfo.SourceNullable)
                            TD.SourceTestColumn = columninfo;
                    }

                    TD.SingleRecord = singleRecord;

                // if multirecord then set link columns
                    if (!singleRecord) {
                        if (identityColumn != null) {
                            identityColumn.Link = true;

                            if (!identityColumn.Source) {
                                context.AddError(Target.n_Where, "Identity column " + SqlStatic.QuoteName(identityColumn.Column.Name) + " missing is source.");
                                return false;
                            }
                        }
                        else {
                            foreach(var columninfo in TD.ColumnsInfo) {
                                if (columninfo.Key) {
                                    if (columninfo.Source) {
                                        if (columninfo.WhereLinkExpr != null) {
                                            context.AddError(Target.n_Where, "Primary-key column " + SqlStatic.QuoteName(columninfo.Column.Name) + " linked by where and source.");
                                            return false;
                                        }

                                        columninfo.Link    = true;
                                        columninfo.Updated = false;
                                    }
                                    else {
                                        if (columninfo.WhereLinkExpr == null) {
                                            context.AddError(Target.n_Where, "Primary-key column " + SqlStatic.QuoteName(columninfo.Column.Name) + " missing in where and source.");
                                            return false;
                                        }
                                    }
                                }
                            }
                        }
                    }

                    if (Target.n_Outputs != null) {
                        if (!singleRecord) {
                            context.AddError(Target.n_Outputs[0], "OUTPUT only allowed single record STORE.");
                            return false;
                        }

                        TD.hasOutput = true;
                    }

                    return true;
                }
                private                 bool                                _filterColumns()
                {
                    bool    rtn = true;

                    var newcolumns = new List<ColumnInfo>();

                    foreach (var ci in TD.ColumnsInfo) {
                        if (ci.Inserted || ci.Updated || ci.Key || ci.Link || ci.RecVersion || ci.WhereLinkExpr != null) {
                            newcolumns.Add(ci);

                            if (ci.Inserted)    TD.hasInsert = true;
                            if (ci.Updated)     TD.hasUpdate = true;
                        }
                    }

                    TD.hasDelete = Target.n_With == null || ! Target.n_With.n_DenyDelete;
                    TD.ColumnsInfo = newcolumns;

                    return rtn;
                }
                private                 ColumnInfo                          _findColumn(DataModel.Column column)
                {
                    foreach(var c in TD.ColumnsInfo) {
                        if (object.ReferenceEquals(c.Column, column))
                            return c;
                    }

                    throw new InvalidOperationException("Can't find column in target column list.");
                }
                private                 DataModel.Index                     _findKey()
                {
                    if (targetTable.Indexes != null) {
                        DataModel.Index found = null;

                        foreach(var index in targetTable.Indexes) {
                            if ((index.Flags & DataModel.IndexFlags.Unique) != 0) {
                                if ((index.Flags & DataModel.IndexFlags.PrimaryKey) != 0)
                                    return index;

                                if ((found == null || found.Columns.Length > index.Columns.Length) && !index.Columns[0].Column.isNullable)
                                    found = index;
                            }
                        }

                        return found;
                    }

                    return null;
                }
            }
        }

        public      readonly    STORE_TARGET[]                      n_Targets;

        private                 string                              _namePrefix;

        public      static      bool                                CanParse(Core.ParserReader reader, IParseContext parseContext)
        {
            return reader.CurrentToken.ID == Core.TokenID.BEGIN && reader.NextPeek().isToken("STORE");
        }
        public                                                      Statement_STORE(Core.ParserReader reader, IParseContext parseContext)
        {
            ParseToken(reader, Core.TokenID.BEGIN);
            ParseToken(reader, "STORE");

            var stores = new List<STORE_TARGET>();

            do {
                stores.Add(AddChild(new STORE_TARGET(reader)));
            }
            while (reader.CurrentToken.isToken("TARGET"));

            n_Targets = stores.ToArray();

            ParseToken(reader, Core.TokenID.END);
            ParseToken(reader, "STORE");
        }

        public      override    void                                TranspileNode(Transpile.Context context)
        {
            _namePrefix = "@ST" + (++context.RootContext.StoreTargetNr).ToString(System.Globalization.CultureInfo.InvariantCulture) + "$";
            n_Targets.TranspileNodes(context);
        }
        public      override    void                                Emit(EmitWriter emitWriter)
        {
            int     i = 0;

            while (!(Children[i] is Core.Token token && token.isToken(TokenID.BEGIN)))
                Children[i++].Emit(emitWriter);

            _emit(emitWriter);

            while (!(Children[i] is Core.Token token && token.isToken(TokenID.END)))
                ++i;

            while (!(Children[i] is Core.Token token && token.isToken("STORE")))
                ++i;

            ++i;

            while (i < Children.Count)
                Children[i++].Emit(emitWriter);
        }

        public                  void                                _emit(EmitWriter emitWriter)
        {
            int i;
            var indent = emitWriter.Linepos;

            for (i = 0 ; i < n_Targets.Length ; ++i)
                n_Targets[i].EmitPre(emitWriter.EmitContext);

            emitWriter.WriteText("BEGIN");

            var errtmpvar = _namePrefix + "err";

            emitWriter.WriteNewLine(indent + 4, "DECLARE ", errtmpvar, " VARCHAR(150)");

            for (i = 0 ; i < n_Targets.Length ; ++i)
                n_Targets[i].EmitDeclare(emitWriter, indent + 4);

            emitWriter.WriteNewLine(-1);
            emitWriter.WriteNewLine(indent+4);
            emitWriter.WriteText("BEGIN TRANSACTION;");

                foreach(var t in n_Targets)
                    t.EmitCompare(emitWriter, indent + 4, errtmpvar);

                for (i = n_Targets.Length - 1 ; i >= 0 ; --i)
                    n_Targets[i].EmitDelete(emitWriter, indent + 4);

                for (i = 0 ; i < n_Targets.Length ; ++i)
                    n_Targets[i].EmitUpdate(emitWriter, indent + 4);

                for (i = 0 ; i < n_Targets.Length ; ++i)
                    n_Targets[i].EmitInsert(emitWriter, indent + 4);

            emitWriter.WriteNewLine(-1);
            emitWriter.WriteNewLine(indent+4);
            emitWriter.WriteText("COMMIT TRANSACTION;");
            emitWriter.WriteNewLine(indent);
            emitWriter.WriteText("END");
        }
    }
}
