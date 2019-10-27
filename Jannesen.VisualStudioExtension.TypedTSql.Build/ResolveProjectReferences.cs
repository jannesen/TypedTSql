using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Microsoft.Build.Evaluation;
using Jannesen.VisualStudioExtension.TypedTSql.Build.Library;

namespace Jannesen.VisualStudioExtension.TypedTSql.Build
{
    public class ResolveProjectReferences: Task
    {
        public              ITaskItem[]                 ProjectReferences               { get; set; }
        public              string                      ProjectDirectory                { get; set; }
        public              bool                        Build                           { get; set; }
        [Output]
        public              ITaskItem[]                 ResolvedProjectReferences       { get; set; }

        public override bool Execute()
        {
            if (ProjectDirectory is null)       throw new ArgumentException("ProjectDirectory not set.");

            try {
                if (ProjectReferences != null) {
                    var     result = new List<ITaskItem>();
                    foreach (var projectReference in ProjectReferences) {
                        ITaskItem   r = null;

                        try {
                            r = _processProjectReference(projectReference);
                        }
                        catch(Exception err) {
                            if (Build)
                                throw;

                            System.Diagnostics.Debug.WriteLine("_processProjectReference failed: " + err.Message);
                        }

                        if (r != null)
                            result.Add(r);
                    }

                    ResolvedProjectReferences = result.ToArray();
                }

                return true;
            }
            catch(Exception err) {
                Log.LogError("Jannesen.VisualstudioExtension.TypedTSql.Build.ResolveProjectReferences failed.", this.GetType().Name, err.Message);
                return false;
            }
        }

        private         ITaskItem           _processProjectReference(ITaskItem projectReference)
        {
            var projectFullPath = Statics.NormelizeFullPath(Path.Combine(ProjectDirectory, projectReference.ItemSpec));
            if (!File.Exists(projectFullPath))
                throw new Exception("Project '" + projectFullPath + "' don't exists.");

            var resolvedItem = new TaskItem(projectFullPath);
            resolvedItem.SetMetadata("OriginalItemSpec", projectReference.ItemSpec);
            resolvedItem.SetMetadata("Name", Path.GetFileNameWithoutExtension(projectFullPath));

            return resolvedItem;
        }
    }
}
