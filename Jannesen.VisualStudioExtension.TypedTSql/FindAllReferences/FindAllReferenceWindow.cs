using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Documents;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.Shell.TableManager;
using Microsoft.VisualStudio.Shell.FindAllReferences;
using Microsoft.VisualStudio.Text.Classification;
using Jannesen.VisualStudioExtension.TypedTSql.Classification;
using Jannesen.VisualStudioExtension.TypedTSql.Library;
using VSInterop            = Microsoft.VisualStudio.Shell.Interop;
using LTTS                 = Jannesen.Language.TypedTSql;

namespace Jannesen.VisualStudioExtension.TypedTSql.FindAllReferences
{
    internal class FindAllReferenceWindow: ITableDataSource
    {
        public const string TypedTSqlFindUsagesTableDataSourceIdentifier            = nameof(TypedTSqlFindUsagesTableDataSourceIdentifier);
        public const string TypedTSqlFindUsagesTableDataSourceSourceTypeIdentifier  = nameof(TypedTSqlFindUsagesTableDataSourceSourceTypeIdentifier);

        public                  string                      DisplayName           => "Find references";
        public                  string                      Identifier            => TypedTSqlFindUsagesTableDataSourceIdentifier;
        public                  string                      SourceTypeIdentifier  => TypedTSqlFindUsagesTableDataSourceSourceTypeIdentifier;

        public  readonly        ClassificationFactory       ClassificationFactory;
        public  readonly        IClassificationFormatMap    ClassificationFormatMap;

        private readonly        IFindAllReferencesWindow    _window;
        private readonly        List<SinkManager>           _sinkManagers;

        internal sealed class SinkManager: IDisposable
        {
            public  readonly    FindAllReferenceWindow  TableDataSource;
            public  readonly    ITableDataSink          Sink;

            public                                      SinkManager(FindAllReferenceWindow tableDataSource, ITableDataSink sink)
            {
                TableDataSource = tableDataSource;
                Sink = sink;
                tableDataSource.AddSinkManager(this);
            }

            public              void                    Dispose()
            {
                TableDataSource.RemoveSinkManager(this);
            }
        }

        public                                          FindAllReferenceWindow(IServiceProvider serviceProvider)
        {
            var componentModel = serviceProvider.GetService<IComponentModel>(typeof(SComponentModel));

            ClassificationFactory   = new ClassificationFactory(componentModel.DefaultExportProvider.GetExportedValue<IClassificationTypeRegistryService>());
            ClassificationFormatMap = componentModel.DefaultExportProvider.GetExportedValue<IClassificationFormatMapService>().GetClassificationFormatMap("tooltip");

            _sinkManagers = new List<SinkManager>();
             
            var findAllReferencesService = serviceProvider.GetService<IFindAllReferencesService>(typeof(SVsFindAllReferences));
            _window = findAllReferencesService.StartSearch("Find reference");
            _window.Manager.AddSource(this,
                                      ContainingColumnDefinition.ColumnName,
                                      UsageColumnDefinition.ColumnName);
        }

        public                  void                    AddEntries(VSInterop.IVsProject ivsProject, List<LTTS.SymbolReferenceList> findResult)
        {
            _window.Title = "'" + findResult[0].Symbol.FullName + "' references";

            foreach(var referenceList in findResult) {
                _addEntries(ivsProject, referenceList);
            }
        }
        public                  void                    AddEntries(VSInterop.IVsProject ivsProject, LTTS.SymbolReferenceList findResult)
        {
            _window.Title = "'" + findResult.Symbol.FullName + "' references";
            _addEntries(ivsProject, findResult);
        }
        public                  IDisposable             Subscribe(ITableDataSink sink)
        {
            return new SinkManager(this, sink);
        }

        internal                void                    AddSinkManager(SinkManager manager)
        {
            lock(_sinkManagers) { 
                _sinkManagers.Add(manager);
            }
        }
        internal                void                    RemoveSinkManager(SinkManager manager)
        {
            lock(_sinkManagers) { 
                _sinkManagers.Remove(manager);
            }
        }

        internal                IEnumerable<Inline>     FormatTextInlines(LTTS.Core.Token[] lineTokens)
        {
            var inlines = new List<Inline>();

            foreach (var t in lineTokens) {
                var run = new Run(t.Text);

                var classificationType = ClassificationFactory.TokenClassificationType(t);

                if (classificationType != null) {
                    var format = ClassificationFormatMap.GetTextProperties(classificationType);
                    if (format != null) {
                        run.SetValue(TextElement.FontFamilyProperty, format.Typeface.FontFamily);
                        run.SetValue(TextElement.FontSizeProperty,   format.FontRenderingEmSize);
                        run.SetValue(TextElement.FontStyleProperty,  format.Italic ? FontStyles.Italic : FontStyles.Normal);
                        run.SetValue(TextElement.FontWeightProperty, format.Bold ? FontWeights.Bold : FontWeights.Normal);
                        run.SetValue(TextElement.BackgroundProperty, format.BackgroundBrush);
                        run.SetValue(TextElement.ForegroundProperty, format.ForegroundBrush);
                    }
                }

                inlines.Add(run);
            }

            return inlines.ToArray();
        }

        private                 void                    _addEntries(VSInterop.IVsProject ivsProject, LTTS.SymbolReferenceList referenceList)
        {
            string projectName = System.IO.Path.GetFileName(VSPackage.GetProjectFileName(ivsProject));
            var entries = new ReferenceEntry[referenceList.Count];

            var definition   = new TypedTSqlDefinitionBucket(referenceList.Symbol);
            for (var i = 0 ; i < referenceList.Count ; ++i) {
                entries[i] = new ReferenceEntry(this, definition, projectName, referenceList[i]);
            }

            lock(_sinkManagers) {
                foreach(var sinkManager in _sinkManagers) {
                    sinkManager.Sink.AddEntries(entries);
                }
            }
        }
    }
}
