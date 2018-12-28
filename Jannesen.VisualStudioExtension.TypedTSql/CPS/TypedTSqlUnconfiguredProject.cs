using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.VS;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Jannesen.VisualStudioExtension.TypedTSql.CPS
{
    [Export]
    [AppliesTo(TypedTSqlUnconfiguredProject.UniqueCapability)]
    [ProjectTypeRegistration(
        projectTypeGuid                 : ProjectTypeGuid,
        displayName                     : "TypedTSql",
        displayProjectFileExtensions    : "#2",
        defaultProjectExtension         : ProjectExtension,
        language                        : Language,
        resourcePackageGuid             : VSPackage.PackageGuid,
        PossibleProjectExtensions       = ProjectExtension,
        ProjectTemplatesDir             = @"ProjectTemplates")]
    [ProvideProjectItem(
        projectFactoryType              : ProjectTypeGuid,
        itemCategoryName                : "My Items",
        templatesDir                    : @"ItemTemplates",
        priority                        : 500)]
    internal class TypedTSqlUnconfiguredProject
    {
        public      const   string                                          ProjectTypeGuid  = "8B06178E-D39A-489A-A914-F6FED88B70C7";
        public      const   string                                          ProjectExtension = "ttsqlproj";
        internal    const   string                                          UniqueCapability = "TypedTSql";
        internal    const   string                                          Language         = "TypedTSql";

        [ImportingConstructor]
        public                                                                      TypedTSqlUnconfiguredProject(UnconfiguredProject unconfiguredProject)
        {
            this.ProjectHierarchies = new OrderPrecedenceImportCollection<IVsHierarchy>(projectCapabilityCheckProvider: unconfiguredProject);
        }

        [Import]
        internal            UnconfiguredProject                                     UnconfiguredProject         { get; private set; }

        [Import]
        internal            IActiveConfiguredProjectSubscriptionService             SubscriptionService         { get; private set; }

        [Import]
        internal            IProjectThreadingService                                ThreadHandling              { get; private set; }

        [Import]
        internal            ActiveConfiguredProject<ConfiguredProject>              ActiveConfiguredProject     { get; private set; }

        [Import]
        internal            ActiveConfiguredProject<TypedTSqlConfiguredProject>     MyActiveConfiguredProject   { get; private set; }

        [ImportMany(ExportContractNames.VsTypes.IVsProject, typeof(IVsProject))]
        internal            OrderPrecedenceImportCollection<IVsHierarchy>           ProjectHierarchies          { get; private set; }

        internal            IVsHierarchy                                            ProjectHierarchy
        {
            get { return this.ProjectHierarchies.Single().Value; }
        }
    }
}
