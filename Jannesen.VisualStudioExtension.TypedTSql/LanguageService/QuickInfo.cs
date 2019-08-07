using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.Text;
using LTTS_Library         = Jannesen.Language.TypedTSql.Library;
using LTTS_Core            = Jannesen.Language.TypedTSql.Core;
using LTTS_DataModel       = Jannesen.Language.TypedTSql.DataModel;
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

        public                                  QuickInfo(ITrackingSpan span, LTTS_DataModel.ISymbol symbol)
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
            Info = _processSymbol(symbol);
        }

        private             UIElement           _processSymbol(LTTS_DataModel.ISymbol symbol)
        {
            try {
                switch(symbol.Type) {
                //case LTTS_DataModel.SymbolType.Assembly:
                case LTTS_DataModel.SymbolType.TypeUser:                                return _processTypeUser((LTTS_DataModel.EntityTypeUser)symbol);
                //case LTTS_DataModel.SymbolType.TypeAssembly:
                //case LTTS_DataModel.SymbolType.TypeTable:
                //case LTTS_DataModel.SymbolType.Default:
                //case LTTS_DataModel.SymbolType.Rule:
                //case LTTS_DataModel.SymbolType.TableInternal:
                //case LTTS_DataModel.SymbolType.TableSystem:
                //case LTTS_DataModel.SymbolType.TableUser:
                //case LTTS_DataModel.SymbolType.Constraint_ForeignKey:
                //case LTTS_DataModel.SymbolType.Constraint_PrimaryKey:
                //case LTTS_DataModel.SymbolType.Constraint_Check:
                //case LTTS_DataModel.SymbolType.Constraint_Unique:
                //case LTTS_DataModel.SymbolType.View:
                //case LTTS_DataModel.SymbolType.Function:
                //case LTTS_DataModel.SymbolType.FunctionScalar:
                //case LTTS_DataModel.SymbolType.FunctionScalar_clr:
                //case LTTS_DataModel.SymbolType.FunctionInlineTable:
                //case LTTS_DataModel.SymbolType.FunctionMultistatementTable:
                //case LTTS_DataModel.SymbolType.FunctionMultistatementTable_clr:
                //case LTTS_DataModel.SymbolType.FunctionAggregateFunction_clr:
                //case LTTS_DataModel.SymbolType.StoredProcedure:
                //case LTTS_DataModel.SymbolType.StoredProcedure_clr:
                //case LTTS_DataModel.SymbolType.StoredProcedure_extended:
                //case LTTS_DataModel.SymbolType.Trigger:
                //case LTTS_DataModel.SymbolType.Trigger_clr:
                //case LTTS_DataModel.SymbolType.PlanGuide:
                //case LTTS_DataModel.SymbolType.ReplicationFilterProcedure:
                //case LTTS_DataModel.SymbolType.SequenceObject:
                //case LTTS_DataModel.SymbolType.ServiceQueue:
                //case LTTS_DataModel.SymbolType.Synonym:
                //case LTTS_DataModel.SymbolType.WebService:
                case LTTS_DataModel.SymbolType.ExternalStaticProperty:                  return _processExternalInterface("external-static-property", (LTTS_DataModel.Interface)symbol);
                case LTTS_DataModel.SymbolType.ExternalStaticMethod:                    return _processExternalInterface("external-static-method",   (LTTS_DataModel.Interface)symbol);
                case LTTS_DataModel.SymbolType.ExternalProperty:                        return _processExternalInterface("external-property",        (LTTS_DataModel.Interface)symbol);
                case LTTS_DataModel.SymbolType.ExternalMethod:                          return _processExternalInterface("external-method",          (LTTS_DataModel.Interface)symbol);
                case LTTS_DataModel.SymbolType.UDTValue:                                return _processUDTValue((LTTS_DataModel.ValueRecord)symbol);
                case LTTS_DataModel.SymbolType.Parameter:                               return _processVariable("parameter", (LTTS_DataModel.Variable)symbol);
                case LTTS_DataModel.SymbolType.Column:                                  return _processColumn((LTTS_DataModel.Column)symbol);
                case LTTS_DataModel.SymbolType.Index:                                   return _processIndex((LTTS_DataModel.Index)symbol);
                case LTTS_DataModel.SymbolType.GlobalVariable:                          return _processVariable("global-variable", (LTTS_DataModel.Variable)symbol);
                case LTTS_DataModel.SymbolType.LocalVariable:                           return _processVariable("variable", (LTTS_DataModel.Variable)symbol);

                //case LTTS_DataModel.SymbolType.BuildinFunction:
                case LTTS_DataModel.SymbolType.RowsetAlias:                             return _processRowAlias((LTTS_DataModel.RowSet)symbol);
                default:
                    {
                        var stackPanel = new StackPanel() { Orientation=Orientation.Horizontal };
                            stackPanel.Children.Add(_textType(Helpers.SymbolTypeToString(symbol.Type)));
                            stackPanel.Children.Add(_textName(symbol.Name));
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

        private             UIElement           _processTypeUser(LTTS_DataModel.EntityTypeUser typeUser)
        {
            var rtn = new StackPanel() { Orientation=Orientation.Vertical };

            rtn.Children.Add(_stackPanelTypeName("user-type",   typeUser.EntityName.Fullname));
            rtn.Children.Add(_stackPanelTypeName("native-type", typeUser.NativeType.ToString()));

            return rtn;
        }
        private             UIElement           _processExternalInterface(string type, LTTS_DataModel.Interface intf)
        {
            var rtn = new StackPanel() { Orientation=Orientation.Vertical };

                rtn.Children.Add(_stackPanelTypeName(type, LTTS_Library.SqlStatic.QuoteName(intf.Name)));

                if (intf.Parent != null)
                    rtn.Children.Add(_stackPanelCategory("parent", _processSymbol(intf.Parent)));

                if (intf.Returns != null)
                    rtn.Children.Add(_stackPanelCategory("returns", _processType(intf.Returns)));

            return rtn;
        }
        private             UIElement           _processUDTValue(LTTS_DataModel.ValueRecord valueRecord)
        {
            var rtn = new StackPanel() { Orientation=Orientation.Vertical };

                rtn.Children.Add(_stackPanelTypeName("udt-value", LTTS_Library.SqlStatic.QuoteName(valueRecord.Name)));
                rtn.Children.Add(_stackPanelTypeName("value", Helpers.ObjectValueToString(valueRecord.Value)));

                if (valueRecord.Parent != null)
                    rtn.Children.Add(_stackPanelCategory("parent", _processSymbol(valueRecord.Parent)));

            return rtn;
        }
        private             UIElement           _processColumn(LTTS_DataModel.Column column)
        {
            var rtn = new StackPanel() { Orientation=Orientation.Vertical };

                rtn.Children.Add(_stackPanelTypeName("column", LTTS_Library.SqlStatic.QuoteName(column.Name)));

                if (column.Parent != null)
                    rtn.Children.Add(_stackPanelCategory("parent", _processSymbol(column.Parent)));

                if (column.SqlType != null)
                    rtn.Children.Add(_stackPanelCategory("type", _processType(column.SqlType)));

            return rtn;
        }
        private             UIElement           _processVariable(string type, LTTS_DataModel.Variable variable)
        {
            var rtn = new StackPanel() { Orientation=Orientation.Vertical };
            rtn.Children.Add(_stackPanelTypeName(type, variable.Name));

            if (variable.SqlType != null)
                rtn.Children.Add(_stackPanelCategory("type", _processType(variable.SqlType)));

            return rtn;
        }
        private             UIElement           _processRowAlias(LTTS_DataModel.RowSet rowset)
        {
            var rtn = new StackPanel() { Orientation=Orientation.Vertical };

                rtn.Children.Add(_stackPanelTypeName("alias",  rowset.Name));

                if (rowset.Source != null)
                    rtn.Children.Add(_stackPanelCategory("source", _processSymbol(rowset.Source)));

            return rtn;
        }
        private             UIElement           _processIndex(LTTS_DataModel.Index index)
        {
            var rtn = new StackPanel() { Orientation=Orientation.Vertical };

                rtn.Children.Add(_stackPanelTypeName("index", LTTS_Library.SqlStatic.QuoteName(index.Name)));

                if (index.Columns != null) {
                    var indexes = new StringBuilder();

                    for (int i = 0 ; i < index.Columns.Length ; ++i) {
                        if (i > 0)
                            indexes.Append(", ");

                        indexes.Append(LTTS_Library.SqlStatic.QuoteName(index.Columns[i].Column.Name));
                    }

                    rtn.Children.Add(_stackPanelTypeName("columns", indexes.ToString()));
                }

                if (index.Filter != null)
                    rtn.Children.Add(_stackPanelTypeName("where", index.Filter));

            return rtn;
        }
        private             UIElement           _processType(LTTS_DataModel.ISqlType sqlType)
        {
            var rtn = new StackPanel() { Orientation=Orientation.Vertical };

                if (sqlType.Entity != null)
                    rtn.Children.Add(_processSymbol(sqlType.Entity));
                else
                if ((sqlType.TypeFlags & LTTS_DataModel.SqlTypeFlags.SimpleType) != 0)
                    rtn.Children.Add(_stackPanelTypeName("native-type", sqlType.NativeType.ToString()));
                else
                if (sqlType is LTTS_DataModel.EntityTypeExternal)
                    rtn.Children.Add(_textName("type-external"));
                else
                if (sqlType is LTTS_DataModel.EntityTypeTable)
                    rtn.Children.Add(_textName("type-table"));
                else
                if (sqlType is LTTS_DataModel.SqlTypeTable)
                    rtn.Children.Add(_textName("table"));
                else
                if (sqlType is LTTS_DataModel.SqlTypeCursorRef)
                    rtn.Children.Add(_textName("cursor-ref"));
                else
                if (sqlType is LTTS_DataModel.SqlTypeAny)
                    rtn.Children.Add(_textName("ANY"));
                else
                if (sqlType is LTTS_DataModel.SqlTypeVoid)
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
