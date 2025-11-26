using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.Text.Adornments;
using LTTS                  = Jannesen.Language.TypedTSql;
using Jannesen.VisualStudioExtension.TypedTSql.Classification;
using Jannesen.VisualStudioExtension.TypedTSql.Library;

namespace Jannesen.VisualStudioExtension.TypedTSql.LanguageService
{
    class QuickInfo
    {
        public              int                         Begin       { get; private set; }
        public              int                         End         { get; private set; }
        public              object                      Info        { get; private set; }

        public                                          QuickInfo(int begin, int end, LTTS.DataModel.SymbolData symbolData)
        {
            Begin = begin;
            End   = end;
            Info  = _processSymbolDate(symbolData, true);
        }

        private static      object                      _processSymbolDate(LTTS.DataModel.SymbolData symbolData, bool details)
        {
            if (symbolData is LTTS.DataModel.SymbolUsage symbolUsage) { 
                return _processSymbol(symbolUsage.Symbol, details);
            }
            if (symbolData is LTTS.DataModel.SymbolSourceTarget symbolSourceTarget) {
                return new ContainerElement(ContainerElementStyle.Wrapped,
                                _processSymbol(symbolSourceTarget.Target.Symbol, details),
                                " = ",
                                _processSymbol(symbolSourceTarget.Source.Symbol, details)
                           );
            }
            if (symbolData is LTTS.DataModel.SymbolWildcard symbolWildcard) { 
                var elements = new List<object>();

                foreach (var item in symbolWildcard.SymbolData) {
                    elements.Add(_processSymbolDate(item, false));
                }

                return new ContainerElement(ContainerElementStyle.Stacked, elements);
            }

            return "Symbol type " + symbolData.GetType().Name + " not implemeted.";
        }
        private static      object                      _processSymbol(LTTS.DataModel.ISymbol symbol, bool details)
        {
            try {
                switch(symbol.Type) {
                //case LTTS.DataModel.SymbolType.Assembly:
                case LTTS.DataModel.SymbolType.TypeUser:                                return _processTypeUser((LTTS.DataModel.EntityTypeUser)symbol);
                //case LTTS.DataModel.SymbolType.TypeAssembly:
                //case LTTS.DataModel.SymbolType.TypeTable:
                //case LTTS.DataModel.SymbolType.Default:
                //case LTTS.DataModel.SymbolType.Rule:
                //case LTTS.DataModel.SymbolType.TableInternal:
                //case LTTS.DataModel.SymbolType.TableSystem:
                //case LTTS.DataModel.SymbolType.TableUser:
                //case LTTS.DataModel.SymbolType.Constraint_ForeignKey:
                //case LTTS.DataModel.SymbolType.Constraint_PrimaryKey:
                //case LTTS.DataModel.SymbolType.Constraint_Check:
                //case LTTS.DataModel.SymbolType.Constraint_Unique:
                //case LTTS.DataModel.SymbolType.View:
                //case LTTS.DataModel.SymbolType.Function:
                //case LTTS.DataModel.SymbolType.FunctionScalar:
                //case LTTS.DataModel.SymbolType.FunctionScalar_clr:
                //case LTTS.DataModel.SymbolType.FunctionInlineTable:
                //case LTTS.DataModel.SymbolType.FunctionMultistatementTable:
                //case LTTS.DataModel.SymbolType.FunctionMultistatementTable_clr:
                //case LTTS.DataModel.SymbolType.FunctionAggregateFunction_clr:
                //case LTTS.DataModel.SymbolType.StoredProcedure:
                //case LTTS.DataModel.SymbolType.StoredProcedure_clr:
                //case LTTS.DataModel.SymbolType.StoredProcedure_extended:
                //case LTTS.DataModel.SymbolType.Trigger:
                //case LTTS.DataModel.SymbolType.Trigger_clr:
                //case LTTS.DataModel.SymbolType.PlanGuide:
                //case LTTS.DataModel.SymbolType.ReplicationFilterProcedure:
                //case LTTS.DataModel.SymbolType.SequenceObject:
                //case LTTS.DataModel.SymbolType.ServiceQueue:
                //case LTTS.DataModel.SymbolType.Synonym:
                //case LTTS.DataModel.SymbolType.WebService:
                case LTTS.DataModel.SymbolType.ExternalStaticProperty:                  return _processExternalInterface("external-static-property", (LTTS.DataModel.Interface)symbol, details);
                case LTTS.DataModel.SymbolType.ExternalStaticMethod:                    return _processExternalInterface("external-static-method",   (LTTS.DataModel.Interface)symbol, details);
                case LTTS.DataModel.SymbolType.ExternalProperty:                        return _processExternalInterface("external-property",        (LTTS.DataModel.Interface)symbol, details);
                case LTTS.DataModel.SymbolType.ExternalMethod:                          return _processExternalInterface("external-method",          (LTTS.DataModel.Interface)symbol, details);
                case LTTS.DataModel.SymbolType.UDTValue:                                return _processUDTValue((LTTS.DataModel.ValueRecord)symbol, details);
                case LTTS.DataModel.SymbolType.Parameter:                               return _processVariable("parameter", (LTTS.DataModel.Variable)symbol, details);
                case LTTS.DataModel.SymbolType.Column:                                  return _processColumn((LTTS.DataModel.Column)symbol, details);
                case LTTS.DataModel.SymbolType.Index:                                   return _processIndex((LTTS.DataModel.Index)symbol, details);
                case LTTS.DataModel.SymbolType.GlobalVariable:                          return _processVariable("global-variable", (LTTS.DataModel.Variable)symbol, details);
                case LTTS.DataModel.SymbolType.LocalVariable:                           return _processVariable("variable", (LTTS.DataModel.Variable)symbol, details);

                //case LTTS.DataModel.SymbolType.BuildinFunction:
                case LTTS.DataModel.SymbolType.RowsetAlias:                             return _processRowAlias((LTTS.DataModel.RowSet)symbol);
                default:
                    {
                        return _textElementTypeName(Helpers.SymbolTypeToString(symbol.Type), symbol.FullName);
                    }
                }
            }
            catch(Exception err) {
                return new ClassifiedTextElement(
                           new ClassifiedTextRun(ClassificationTypes.Error, "Type: " + Helpers.SymbolTypeToString(symbol.Type) + " ERROR: " + err.Message)
                       );
            }
        }
        private static      ContainerElement            _processTypeUser(LTTS.DataModel.EntityTypeUser typeUser)
        {
            return new ContainerElement(ContainerElementStyle.Stacked,
                                        _textElementTypeName("user-type",   typeUser.EntityName.Fullname),
                                        _textElementTypeName("native-type", typeUser.NativeType.ToString()));
        }
        private static      ContainerElement            _processExternalInterface(string type, LTTS.DataModel.Interface intf, bool details)
        {
            var elments = new List<object>();

            elments.Add(_textElementTypeName(type, LTTS.Library.SqlStatic.QuoteName(intf.Name)));

            if (details) {
                if (intf.ParentSymbol != null) {
                    elments.Add(_elementPanelCategory("parent", _processSymbol(intf.ParentSymbol, false)));
                }

                if (intf.Returns != null) {
                    elments.Add(_elementPanelCategory("returns", _processType(intf.Returns)));
                }
            }

            return new ContainerElement(ContainerElementStyle.Stacked, elments);
        }
        private static      ContainerElement            _processUDTValue(LTTS.DataModel.ValueRecord valueRecord, bool details)
        {
            var elments = new List<object>();

            elments.Add(_textElementTypeName("udt-value", LTTS.Library.SqlStatic.QuoteName(valueRecord.Name)));
            elments.Add(_textElementTypeName("value", Helpers.ObjectValueToString(valueRecord.Value)));

            if (details) {
                if (valueRecord.ParentSymbol != null) {
                    elments.Add(_elementPanelCategory("parent", _processSymbol(valueRecord.ParentSymbol, false)));
                }
            }

            return new ContainerElement(ContainerElementStyle.Stacked, elments);
        }
        private static      ContainerElement            _processColumn(LTTS.DataModel.Column column, bool details)
        {
            var elments = new List<object>();

            elments.Add(_textElementTypeName("column", LTTS.Library.SqlStatic.QuoteName(column.Name)));

            if (details) {
                if (column.SqlType != null) {
                    elments.Add(_elementPanelCategory("type", _processType(column.SqlType)));
                }

                if (column.ParentSymbol != null) {
                    elments.Add(_elementPanelCategory("parent", _processSymbol(column.ParentSymbol, false)));
                }

                elments.Add(_elementTextNullable(column.isNullable));
            }

            return new ContainerElement(ContainerElementStyle.Stacked, elments);
        }
        private static      ContainerElement            _processVariable(string type, LTTS.DataModel.Variable variable, bool details)
        {
            var elments = new List<object>();

            elments.Add(_textElementTypeName(type, variable.Name));

            if (details) {
                if (variable.SqlType != null) {
                    elments.Add(_elementPanelCategory("type", _processType(variable.SqlType)));
                }

                elments.Add(_elementTextNullable(variable.isNullable));
            }

            return new ContainerElement(ContainerElementStyle.Stacked, elments);
        }
        private static      ContainerElement            _processRowAlias(LTTS.DataModel.RowSet rowset)
        {
            var elments = new List<object>();

            elments.Add(_elementPanelCategory("alias",  rowset.Name));

            if (rowset.Source != null) {
                elments.Add(_elementPanelCategory("source", _processSymbol(rowset.Source, true)));
            }

            elments.Add(_elementTextNullable(rowset.isNullable));

            return new ContainerElement(ContainerElementStyle.Stacked, elments);
        }
        private static      ContainerElement            _processIndex(LTTS.DataModel.Index index, bool details)
        {
            var elments = new List<object>();

            elments.Add(_textElementTypeName("index", LTTS.Library.SqlStatic.QuoteName(index.Name)));

            if (details) {
                if (index.Columns != null) {
                    var indexes = new StringBuilder();

                    for (int i = 0 ; i < index.Columns.Length ; ++i) {
                        if (i > 0)
                            indexes.Append(", ");

                        indexes.Append(LTTS.Library.SqlStatic.QuoteName(index.Columns[i].Column.Name));
                    }

                    elments.Add(_textElementTypeName("columns", indexes.ToString()));
                }

                if (index.Filter != null) {
                    elments.Add(_textElementTypeName("where", index.Filter));
                }
            }

            return new ContainerElement(ContainerElementStyle.Stacked, elments);
        }

        private static      ContainerElement            _processType(LTTS.DataModel.ISqlType sqlType)
        {
            var elements = new List<object>();

            if (sqlType.Entity != null)
                elements.Add(_processSymbol(sqlType.Entity, true));
            else
            if ((sqlType.TypeFlags & LTTS.DataModel.SqlTypeFlags.SimpleType) != 0)
                elements.Add(_textElementTypeName("native-type", sqlType.NativeType.ToString()));
            else
            if (sqlType is LTTS.DataModel.EntityTypeExternal)
                elements.Add(_textElementName("type-external"));
            else
            if (sqlType is LTTS.DataModel.EntityTypeTable)
                elements.Add(_textElementName("type-table"));
            else
            if (sqlType is LTTS.DataModel.SqlTypeTable)
                elements.Add(_textElementName("table"));
            else
            if (sqlType is LTTS.DataModel.SqlTypeCursorRef)
                elements.Add(_textElementName("cursor-ref"));
            else
            if (sqlType is LTTS.DataModel.SqlTypeAny)
                elements.Add(_textElementName("ANY"));
            else
            if (sqlType is LTTS.DataModel.SqlTypeVoid)
                elements.Add(_textElementName("VOID"));

            return new ContainerElement(ContainerElementStyle.Stacked, elements);
        }
        private static      ContainerElement            _elementPanelCategory(string category, object panel)
        {
            return new ContainerElement(ContainerElementStyle.Wrapped,
                                        _textElementComment(category + " "),
                                        panel);
        }
        private static      ContainerElement            _elementTextNullable(bool nullable)
        {
            return _elementPanelCategory("nullable", _textElementComment(nullable ? "yes" : "no"));
        }
        private static      ClassifiedTextElement       _textElementComment(string name)
        {
            return new ClassifiedTextElement(
                       new ClassifiedTextRun(ClassificationTypes.Comment, name)
                   );
        }
        private static      ClassifiedTextElement       _textElementName(string name)
        {
            return new ClassifiedTextElement(
                       new ClassifiedTextRun(ClassificationTypes.Name, name)
                   );
        }
        private static      ClassifiedTextElement       _textElementTypeName(string type, string name)
        {
            return new ClassifiedTextElement(
                       new ClassifiedTextRun(ClassificationTypes.Type, type + " "),
                       new ClassifiedTextRun(ClassificationTypes.Name, name)
                   );
        }
    }
}
