using System;
using System.ComponentModel.Composition;
using System.Diagnostics.CodeAnalysis;
using Microsoft.VisualStudio.ProjectSystem;

namespace Jannesen.VisualStudioExtension.TypedTSql.CPS
{
    [Export]
    [AppliesTo(TypedTSqlUnconfiguredProject.UniqueCapability)]
    internal class TypedTSqlConfiguredProject
    {
        [Import, SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "MEF")]
        internal        ConfiguredProject       ConfiguredProject   { get; private set; }

        [Import, SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification = "MEF")]
        internal        ProjectProperties       Properties          { get; private set; }

        public                                  TypedTSqlConfiguredProject()
        {
        }
    }
}
