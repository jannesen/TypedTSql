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
        public              string                      ProjectConfiguration            { get; set; }
        public              string                      ProjectPlatform                 { get; set; }
        public              bool                        Build                           { get; set; }
        [Output]
        public              ITaskItem[]                 ResolvedProjectReferences       { get; set; }

        public override bool Execute()
        {
            if (ProjectDirectory == null)       throw new ArgumentException("ProjectDirectory not set.");
            if (ProjectConfiguration == null)   throw new ArgumentException("ProjectConfiguration not set.");
            if (ProjectPlatform == null)        throw new ArgumentException("ProjectPlatform not set.");

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

            Project project = null;

            var loadedProjects = ProjectCollection.GlobalProjectCollection.GetLoadedProjects(projectFullPath);
            if (loadedProjects.Count > 0) {
                foreach (var loaded in loadedProjects) {
                    loaded.GlobalProperties.TryGetValue("Configuration", out var configuration);
                    loaded.GlobalProperties.TryGetValue("Platform",      out var platform);

                    if (configuration == ProjectConfiguration && platform == ProjectPlatform)
                        project = loaded;
                }

                if (project == null)
                    throw new Exception("Can't locate project '" + projectFullPath + "' with active Configuration.");
            }
            else {
                project = new Project(projectFullPath,
                                      new Dictionary<string,string>() {
                                           ["Configuration"] = ProjectConfiguration,
                                           ["Platform"]= ProjectPlatform
                                      }, null);
            }

            var resolvedItem = new TaskItem(projectReference.ItemSpec);
            var propertyValue = new Dictionary<string, string>();
            string value;

            foreach(var pp in project.AllEvaluatedProperties)
                propertyValue[pp.Name] = propertyValue.TryGetValue(pp.Name, out value) ? value+";"+pp.EvaluatedValue : pp.EvaluatedValue;

            if (propertyValue.TryGetValue("ProjectGuid", out value))
                resolvedItem.SetMetadata("Project", value);

            if (propertyValue.TryGetValue("OutputType", out value) && value == "Library") {
                if (propertyValue.TryGetValue("TargetPath", out value)) {
                    resolvedItem.SetMetadata("ProjectType",    "Assembly");
                    resolvedItem.SetMetadata("OutputAssembly", Statics.NormelizeFullPath(Path.Combine(Path.GetDirectoryName(projectFullPath), value)));
                }
            }

            return resolvedItem;
        }
    }
}
