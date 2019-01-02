using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using LTTS                 = Jannesen.Language.TypedTSql;

namespace Jannesen.VisualStudioExtension.TypedTSql.LanguageService
{
    internal sealed class ErrorList: IDisposable
    {
        private                 Service                                         _service;
        private                 IVsProject                                      _vsproject;
        private                 Dictionary<LTTS.TypedTSqlMessage, ErrorTask>    _activeError;

        public                                                                  ErrorList(Service service, IVsProject vsproject)
        {
            _service     = service;
            _vsproject   = vsproject;
            _activeError = new Dictionary<LTTS.TypedTSqlMessage, ErrorTask>();
        }
        public                  void                                            Dispose()
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var errors = _activeError;
            _activeError = null;

            var tasks = _service.ErrorListProvider?.Tasks;
            if (tasks != null) {
                foreach(var r in errors.Values)
                    tasks.Remove(r);
            }

        }

        public                  void                                            Update(IReadOnlyList<LTTS.TypedTSqlMessage> errors)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var tasks = _service.ErrorListProvider?.Tasks;
            if (tasks != null) {
                foreach (var error in errors) {
                    if (!_activeError.ContainsKey(error)) {
                        var errorTask = new  ErrorTask()
                                {
                                    Document      = error.SourceFile.Filename,
                                    Line          = error.Beginning.Lineno  - 1,
                                    Column        = error.Beginning.Linepos - 1,
                                    Text          = error.QuickFix != null ? error.Message + " [quickfix]" : error.Message,
                                    ErrorCategory = (error.Classification == LTTS.TypedTSqlMessageClassification.TranspileWarning) ? TaskErrorCategory.Warning : TaskErrorCategory.Error,
                                    Category      = TaskCategory.CodeSense,
                                    HierarchyItem = _vsproject as IVsHierarchy
                                };

                        errorTask.Navigate += _onErrorNavigate;

                        _activeError.Add(error, errorTask);
                        tasks.Add(errorTask);
                    }
                }

                var errorsHashset = new HashSet<LTTS.TypedTSqlMessage>(errors);
                var keysToRemove  = new List<LTTS.TypedTSqlMessage>();

                foreach(var r in _activeError) {
                    if (!errorsHashset.Contains(r.Key)) {
                        tasks.Remove(r.Value);
                        keysToRemove.Add(r.Key);
                    }
                }

                foreach(var k in keysToRemove)
                    _activeError.Remove(k);
            }
        }
        public                  LTTS.TypedTSqlMessage                           GetMMessageAt(string fullpath, int filePosition)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            foreach(var e in _activeError.Keys) {
                if (e != null &&
                    e.SourceFile.Filename == fullpath &&
                    e.Beginning.Filepos <= filePosition &&
                    e.Ending.Filepos    >  filePosition)
                    return e;
            }

            throw new Exception("No (error) message at location.");
        }

        private                 void                                            _onErrorNavigate(object sender, EventArgs e)
        {
            if (sender is ErrorTask errorTask)
                VSPackage.NavigateTo(_vsproject, errorTask.Document, errorTask.Line + 1, errorTask.Column + 1);
        }
    }
}
