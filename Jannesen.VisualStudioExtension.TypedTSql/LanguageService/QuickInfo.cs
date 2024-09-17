using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using LTTS                  = Jannesen.Language.TypedTSql;
using Jannesen.VisualStudioExtension.TypedTSql.Library;

namespace Jannesen.VisualStudioExtension.TypedTSql.LanguageService
{
    class QuickInfo
    {
        public              ITrackingSpan       Span { get; private set; }
        public              UIElement           Info { get; private set; }
        public              Brush               _colorCategory;
        public              Brush               _colorType;
        public              Brush               _colorName;

        public                                  QuickInfo(ITrackingSpan span, LTTS.DataModel.SymbolData symbolData)
        {
            switch(VSPackage.GetCurrentTheme()) {
            case VSPackage.ColorTheme.Dark:
                _colorCategory = Brushes.MediumSeaGreen;
                _colorType     = Brushes.CornflowerBlue;
                _colorName     = Brushes.White;
                break;
            case VSPackage.ColorTheme.Light:
            case VSPackage.ColorTheme.Blue:
            default:
                _colorCategory = Brushes.MediumSeaGreen;
                _colorType     = Brushes.MediumBlue;
                _colorName     = Brushes.Black;
                break;
            }

            Span = span;
            Info = _processSymbolDate(symbolData, true);
        }

        private             UIElement           _processSymbolDate(LTTS.DataModel.SymbolData symbolData, bool details)
        {
            if (symbolData is LTTS.DataModel.SymbolUsage symbolUsage) { 
                return _processSymbol(symbolUsage.Symbol, details);
            }
            if (symbolData is LTTS.DataModel.SymbolSourceTarget symbolSourceTarget) { 
                var info = new StackPanel() { Orientation=Orientation.Horizontal };
                info.Children.Add(_processSymbol(symbolSourceTarget.Target.Symbol, details));
                info.Children.Add(_textType(" = "));
                info.Children.Add(_processSymbol(symbolSourceTarget.Source.Symbol, details));
                return info;
            }
            if (symbolData is LTTS.DataModel.SymbolWildcard symbolWildcard) { 
                var info = new StackPanel() { Orientation=Orientation.Vertical };

                foreach (var item in symbolWildcard.SymbolData) {
                    info.Children.Add(_processSymbolDate(item, false));
                }
                return info;
            }
            return null;
        }

        private             UIElement           _processSymbol(LTTS.DataModel.ISymbol symbol, bool details)
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
                        var stackPanel = new StackPanel() { Orientation=Orientation.Horizontal };
                            stackPanel.Children.Add(_textType(Helpers.SymbolTypeToString(symbol.Type)));
                            stackPanel.Children.Add(_textName(symbol.FullName));
                        return stackPanel;
                    }
                }
            }
            catch(Exception err) {
                var stackPanel = new StackPanel() { Orientation=Orientation.Horizontal, Background=Brushes.Red };
                    stackPanel.Children.Add(new TextBox() { Text = Helpers.SymbolTypeToString(symbol.Type), Foreground=Brushes.White, Background=Brushes.Transparent, BorderThickness=new Thickness(0) });
                    stackPanel.Children.Add(new TextBox() { Text = "ERROR: " + err.Message,          Foreground=Brushes.White, Background=Brushes.Transparent, BorderThickness=new Thickness(0) });
                return stackPanel;
            }
        }

        private             UIElement           _processTypeUser(LTTS.DataModel.EntityTypeUser typeUser)
        {
            var rtn = new StackPanel() { Orientation=Orientation.Vertical };

            rtn.Children.Add(_stackPanelTypeName("user-type",   typeUser.EntityName.Fullname));
            rtn.Children.Add(_stackPanelTypeName("native-type", typeUser.NativeType.ToString()));

            return rtn;
        }
        private             UIElement           _processExternalInterface(string type, LTTS.DataModel.Interface intf, bool details)
        {
            var rtn = new StackPanel() { Orientation=Orientation.Vertical };

                rtn.Children.Add(_stackPanelTypeName(type, LTTS.Library.SqlStatic.QuoteName(intf.Name)));

                if (details) {
                    if (intf.ParentSymbol != null)
                        rtn.Children.Add(_stackPanelCategory("parent", _processSymbol(intf.ParentSymbol, false)));

                    if (intf.Returns != null)
                        rtn.Children.Add(_stackPanelCategory("returns", _processType(intf.Returns)));
                }
            return rtn;
        }
        private             UIElement           _processUDTValue(LTTS.DataModel.ValueRecord valueRecord, bool details)
        {
            var rtn = new StackPanel() { Orientation=Orientation.Vertical };

                rtn.Children.Add(_stackPanelTypeName("udt-value", LTTS.Library.SqlStatic.QuoteName(valueRecord.Name)));
                rtn.Children.Add(_stackPanelTypeName("value", Helpers.ObjectValueToString(valueRecord.Value)));

                if (details) {
                    if (valueRecord.ParentSymbol != null)
                        rtn.Children.Add(_stackPanelCategory("parent", _processSymbol(valueRecord.ParentSymbol, false)));
                }

            return rtn;
        }
        private             UIElement           _processColumn(LTTS.DataModel.Column column, bool details)
        {
            var rtn = new StackPanel() { Orientation=Orientation.Vertical };

                rtn.Children.Add(_stackPanelTypeName("column", LTTS.Library.SqlStatic.QuoteName(column.Name)));

                if (details) {
                    if (column.SqlType != null)
                        rtn.Children.Add(_stackPanelCategory("type", _processType(column.SqlType)));

                    if (column.ParentSymbol != null)
                        rtn.Children.Add(_stackPanelCategory("parent", _processSymbol(column.ParentSymbol, false)));

                    rtn.Children.Add(_stackPanelCategory("nullable", _textCatagory(column.isNullable ? "yes" : "no")));
                }
            return rtn;
        }
        private             UIElement           _processVariable(string type, LTTS.DataModel.Variable variable, bool details)
        {
            var rtn = new StackPanel() { Orientation=Orientation.Vertical };
            rtn.Children.Add(_stackPanelTypeName(type, variable.Name));

            if (details) {
                if (variable.SqlType != null)
                    rtn.Children.Add(_stackPanelCategory("type", _processType(variable.SqlType)));

                rtn.Children.Add(_stackPanelCategory("nullable", _textCatagory(variable.isNullable ? "yes" : "no")));
            }

            return rtn;
        }
        private             UIElement           _processRowAlias(LTTS.DataModel.RowSet rowset)
        {
            var rtn = new StackPanel() { Orientation=Orientation.Vertical };

                rtn.Children.Add(_stackPanelTypeName("alias",  rowset.Name));

                if (rowset.Source != null)
                    rtn.Children.Add(_stackPanelCategory("source", _processSymbol(rowset.Source, true)));

                rtn.Children.Add(_stackPanelCategory("nullable", _textCatagory(rowset.isNullable ? "yes" : "no")));

            return rtn;
        }
        private             UIElement           _processIndex(LTTS.DataModel.Index index, bool details)
        {
            var rtn = new StackPanel() { Orientation=Orientation.Vertical };

                rtn.Children.Add(_stackPanelTypeName("index", LTTS.Library.SqlStatic.QuoteName(index.Name)));

                if (details) {
                    if (index.Columns != null) {
                        var indexes = new StringBuilder();

                        for (int i = 0 ; i < index.Columns.Length ; ++i) {
                            if (i > 0)
                                indexes.Append(", ");

                            indexes.Append(LTTS.Library.SqlStatic.QuoteName(index.Columns[i].Column.Name));
                        }

                        rtn.Children.Add(_stackPanelTypeName("columns", indexes.ToString()));
                    }

                    if (index.Filter != null)
                        rtn.Children.Add(_stackPanelTypeName("where", index.Filter));
                }

            return rtn;
        }
        private             UIElement           _processType(LTTS.DataModel.ISqlType sqlType)
        {
            var rtn = new StackPanel() { Orientation=Orientation.Vertical };

                if (sqlType.Entity != null)
                    rtn.Children.Add(_processSymbol(sqlType.Entity, true));
                else
                if ((sqlType.TypeFlags & LTTS.DataModel.SqlTypeFlags.SimpleType) != 0)
                    rtn.Children.Add(_stackPanelTypeName("native-type", sqlType.NativeType.ToString()));
                else
                if (sqlType is LTTS.DataModel.EntityTypeExternal)
                    rtn.Children.Add(_textName("type-external"));
                else
                if (sqlType is LTTS.DataModel.EntityTypeTable)
                    rtn.Children.Add(_textName("type-table"));
                else
                if (sqlType is LTTS.DataModel.SqlTypeTable)
                    rtn.Children.Add(_textName("table"));
                else
                if (sqlType is LTTS.DataModel.SqlTypeCursorRef)
                    rtn.Children.Add(_textName("cursor-ref"));
                else
                if (sqlType is LTTS.DataModel.SqlTypeAny)
                    rtn.Children.Add(_textName("ANY"));
                else
                if (sqlType is LTTS.DataModel.SqlTypeVoid)
                    rtn.Children.Add(_textName("VOID"));

            return rtn;
        }
        private             StackPanel          _stackPanelTypeName(string type, string name)
        {
            var rtn = new StackPanel() { Orientation=Orientation.Horizontal };
            rtn.Children.Add(_textType(type));
            rtn.Children.Add(_textName(name));
            return rtn;
        }
        private             StackPanel          _stackPanelCategory(string category, UIElement panel)
        {
            var rtn = new StackPanel() { Orientation=Orientation.Horizontal };

            rtn.Children.Add(_textCatagory(category));
            rtn.Children.Add(panel);

            return rtn;
        }
        private             TextBox             _textCatagory(string text)
        {
            return new TextBox()
                        {
                            Text            = text,
                            Foreground      = _colorCategory,
                            Background      = Brushes.Transparent,
                            BorderThickness = new Thickness(0)
                        };
        }
        private             TextBox             _textType(string text)
        {
            return new TextBox()
                        {
                            Text            = text,
                            Foreground      = _colorType,
                            Background      = Brushes.Transparent,
                            BorderThickness = new Thickness(0)
                        };
        }
        private             TextBox             _textName(string text)
        {
            return new TextBox()
                        {
                            Text            = text,
                            Foreground      = _colorName,
                            Background      = Brushes.Transparent,
                            BorderThickness = new Thickness(0)
                        };
        }
    }
}
