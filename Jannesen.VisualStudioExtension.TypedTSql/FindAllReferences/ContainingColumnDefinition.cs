using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Utilities;

namespace Jannesen.VisualStudioExtension.TypedTSql.FindAllReferences
{
    [Export(typeof(ITableColumnDefinition))]
    [Name(ColumnName)]
    internal class ContainingColumnDefinition: TableColumnDefinitionBase
    {
        public  const           string      ColumnName      = "ContainingInfoPropertyName";

        public  override        bool        IsFilterable    => true;
        public  override        string      Name            => ColumnName;
        public  override        string      DisplayName     => "Containing";
    }
}
