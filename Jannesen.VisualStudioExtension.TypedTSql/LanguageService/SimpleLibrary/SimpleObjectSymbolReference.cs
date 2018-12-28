using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Jannesen.VisualStudioExtension.TypedTSql.Library;
using VSThreadHelper       = Microsoft.VisualStudio.Shell.ThreadHelper;
using LTTS                 = Jannesen.Language.TypedTSql;

namespace Jannesen.VisualStudioExtension.TypedTSql.LanguageService.Library
{
    class SimpleObjectSymbolReference: SimpleObject
    {
        private                 IVsProject              _project;
        private                 LTTS.SymbolReference    _symbolReference;

        public                                          SimpleObjectSymbolReference(IVsProject project, LTTS.SymbolReference symbolReference)
        {
            this._project         = project;
            this._symbolReference = symbolReference;
        }

        public      override    bool                    CanGoToSource(VSOBJGOTOSRCTYPE srcType)
        {
            return srcType == VSOBJGOTOSRCTYPE.GS_REFERENCE;
        }
        public      override    int                     GetDisplayData(VSTREEDISPLAYDATA[] pData)
        {
            pData[0].Image         =
            pData[0].SelectedImage = (ushort)StandardGlyphGroup.GlyphReference;

            return VSConstants.S_OK;
        }

        public      override    string                  GetText(VSTREETEXTOPTIONS tto)
        {
            switch(tto)
            {
            case VSTREETEXTOPTIONS.TTO_DEFAULT:
            case VSTREETEXTOPTIONS.TTO_SEARCHTEXT:
            case VSTREETEXTOPTIONS.TTO_SORTTEXT:
                return this._symbolReference.ToString();

            default:
                return "";
            }
        }
        public      override    int                     GoToSource(VSOBJGOTOSRCTYPE srcType)
        {
            switch(srcType)
            {
            case VSOBJGOTOSRCTYPE.GS_REFERENCE:
                // When navigating with a mouse click in de Find Symbol Result window. Opening a document gives a E_ABORT error.
                // Work around a problem using to navigate async.
                // Normal code: return VSPackage.NavigateTo(_project, _symbolReference.DocumentSpan) ? VSConstants.S_OK : VSConstants.E_FAIL;
                Task.Run(async() =>
                            {
                                await VSThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                                VSPackage.NavigateTo(_project, _symbolReference.DocumentSpan);
                            });
                return VSConstants.S_OK;

            default:
                return VSConstants.E_FAIL;
            }
        }
    }
}
