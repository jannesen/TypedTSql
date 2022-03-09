using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace Jannesen.VisualStudioExtension.TypedTSql.Editor.Classifier
{
    [Export(typeof(IClassifierProvider))]
    [ContentType(FileAndContentTypeDefinitions.TypedTSqlContentTypeName)]
    internal class ClassifierProvider: IClassifierProvider
    {
        [Import]
        private                     IClassificationTypeRegistryService          classificationRegistry = null;
        [Import]
        private                     SVsServiceProvider                          ServiceProvider = null;

        public                      IClassifier                                 GetClassifier(ITextBuffer textBuffer)
        {
            return textBuffer.Properties.GetOrCreateSingletonProperty<Classifier>(typeof(Classifier), () => new Classifier(ServiceProvider, textBuffer, this.classificationRegistry));
        }
    }
}
