using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Utilities;

namespace Jannesen.VisualStudioExtension.TypedTSql.FindAllReferences
{
    [Export(typeof(ITableColumnDefinition))]
    [Name(ColumnName)]
    internal class UsageColumnDefinition: TableColumnDefinitionBase
    {
        public  const           string      ColumnName      = "UsageInfoPropertyName";

        public  override        bool        IsFilterable    => true;
        public  override        string      Name            => ColumnName;
        public  override        string      DisplayName     => "Usage";
    }
}
