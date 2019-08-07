using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Jannesen.VisualStudioExtension.TypedTSql.Rename
{
    interface IPreviewItem
    {
        bool                        IsExpandable                            { get; }
        PreviewList                 Children                                { get; }
        void                        GetDisplayData(ref VSTREEDISPLAYDATA data);
        string                      GetText(VSTREETEXTOPTIONS options);
        _VSTREESTATECHANGEREFRESH   ToggleState();
        void                        DisplayPreview(IVsTextView view);
    }

    class PreviewList: IVsLiteTreeList, IVsPreviewChangesList
    {
        public  readonly        IPreviewItem[]          Items;

        public                                          PreviewList(IPreviewItem[] items) {
            this.Items = items;
        }

        public                  int                     GetDisplayData(uint index, VSTREEDISPLAYDATA[] pData)
        {
            if (index < 0 || index >= Items.Length)
                return VSConstants.E_FAIL;

            Items[index].GetDisplayData(ref pData[0]);
            return VSConstants.S_OK;
        }
        public                  int                     GetExpandable(uint index, out int pfExpandable)
        {
            if (index < 0 || index >= Items.Length) {
                pfExpandable = 0;
                return VSConstants.E_FAIL;
            }

            pfExpandable = Items[index].IsExpandable ? 1 : 0;
            return VSConstants.S_OK;
        }
        public                  int                     GetExpandedList(uint index, out int pfCanRecurse, out IVsLiteTreeList pptlNode)
        {
            if (index < 0 || index >= Items.Length) {
                pfCanRecurse = 0;
                pptlNode     = null;
                return VSConstants.E_FAIL;
            }

            var item = Items[index];
            pptlNode = item.Children;
            pfCanRecurse = item.IsExpandable ? 1 : 0;
            return VSConstants.S_OK;
        }
        public                  int                     GetFlags(out uint pFlags)
        {
            pFlags = 0;
            return VSConstants.S_OK;
        }
        public                  int                     GetItemCount(out uint pCount)
        {
            pCount = (uint)Items.Length;
            return VSConstants.S_OK;
        }
        public                  int                     GetListChanges(ref uint pcChanges, VSTREELISTITEMCHANGE[] prgListChanges)
        {
            if (prgListChanges == null) {
                pcChanges = (uint)Items.Length;
                return VSConstants.S_OK;
            }

            for (int i = 0; i < pcChanges; i++) {
                prgListChanges[i].grfChange = (uint)_VSTREEITEMCHANGESMASK.TCT_ITEMNAMECHANGED;
                prgListChanges[i].index = (uint)i;
            }

            return VSConstants.S_OK;
        }
        public                  int                     GetText(uint index, VSTREETEXTOPTIONS tto, out string ppszText)
        {
            ppszText = Items[index].GetText(tto);
            return VSConstants.S_OK;
        }
        public                  int                     GetTipText(uint index, VSTREETOOLTIPTYPE eTipType, out string ppszText)
        {
            ppszText = null;
            return VSConstants.E_NOTIMPL;
        }
        public                  int                     LocateExpandedList(IVsLiteTreeList ExpandedList, out uint iIndex)
        {
            iIndex = 0;
            return VSConstants.S_FALSE;
        }
        public                  int                     OnClose(VSTREECLOSEACTIONS[] ptca)
        {
            return VSConstants.S_OK;
        }
        public                  int                     OnRequestSource(uint index, object pIUnknownTextView)
        {
            if (pIUnknownTextView == null)
                return VSConstants.E_POINTER;

            if (!(pIUnknownTextView is IVsTextView view))
                return VSConstants.E_NOINTERFACE;

            Items[index].DisplayPreview(view);

            return VSConstants.S_OK;
        }
        public                  int                     ToggleState(uint index, out uint ptscr)
        {
            var item = Items[index];
            ptscr = (uint)item.ToggleState();
            return VSConstants.S_OK;
        }
        public                  int                     UpdateCounter(out uint pCurUpdate, out uint pgrfChanges)
        {
            pCurUpdate  = 0;
            pgrfChanges = 0;
            return VSConstants.E_NOTIMPL;
        }
    }
}
