using System.Collections.Immutable;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.References;
using Microsoft;

namespace Jannesen.VisualStudioExtension.TypedTSql.CPS.Reference
{
    [Export(typeof(IValidProjectReferenceChecker))]
    [AppliesTo(TypedTSqlUnconfiguredProject.UniqueCapability)]
    [Order(/*Order.Default*/10)]
    internal class AlwaysAllowValidProjectReferenceChecker : IValidProjectReferenceChecker
    {
        [ImportingConstructor]
        public AlwaysAllowValidProjectReferenceChecker()
        {
        }

        public Task<SupportedCheckResult> CanAddProjectReferenceAsync(object referencedProject)
        {
            Requires.NotNull(referencedProject, nameof(referencedProject));
            return Task.FromResult(SupportedCheckResult.Supported);
        }

        public Task<CanAddProjectReferencesResult> CanAddProjectReferencesAsync(IImmutableSet<object> referencedProjects)
        {
            Requires.NotNullEmptyOrNullElements(referencedProjects, nameof(referencedProjects));

            IImmutableDictionary<object, SupportedCheckResult> results = ImmutableDictionary.Create<object, SupportedCheckResult>();

            foreach (object referencedProject in referencedProjects) {
                results = results.Add(referencedProject, SupportedCheckResult.Supported);
            }

            return Task.FromResult(new CanAddProjectReferencesResult(results, null));
        }

        public Task<SupportedCheckResult> CanBeReferencedAsync(object referencingProject)
        {
            Requires.NotNull(referencingProject, nameof(referencingProject));
            return Task.FromResult(SupportedCheckResult.Supported);
        }
    }
}
