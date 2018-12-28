using System;
using System.Collections.Generic;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;

namespace Jannesen.VisualStudioExtension.TypedTSql.LanguageService.Library
{
    internal class SimpleObjectList: IVsSimpleObjectList2
    {
        private             List<SimpleObject>          _items;

        public                                          SimpleObjectList(List<SimpleObject> items)
        {
            _items = items;
        }

        int IVsSimpleObjectList2.CanDelete(uint index, out int pfOK)
        {
            pfOK = 0;
            return VSConstants.E_NOTIMPL;
        }
        int IVsSimpleObjectList2.CanGoToSource(uint index, VSOBJGOTOSRCTYPE srcType, out int pfOK)
        {
            if (index >= _items.Count) {
                pfOK = 0;
                return VSConstants.E_INVALIDARG;
            }

            pfOK = _items[(int)index].CanGoToSource(srcType) ? 1 : 0;
            return VSConstants.S_OK;
        }
        int IVsSimpleObjectList2.CanRename(uint index, string pszNewName, out int pfOK)
        {
            pfOK = 0;
            return VSConstants.E_NOTIMPL;
        }
        int IVsSimpleObjectList2.CountSourceItems(uint index, out IVsHierarchy ppHier, out uint pItemid, out uint pcItems)
        {
            if (index >= _items.Count) {
                ppHier = null;
                pItemid = 0;
                pcItems = 0;
                return VSConstants.E_INVALIDARG;
            }

            return _items[(int)index].CountSourceItems(out ppHier, out pItemid, out pcItems);
        }
        int IVsSimpleObjectList2.DoDelete(uint index, uint grfFlags)
        {
            return VSConstants.E_NOTIMPL;
        }
        int IVsSimpleObjectList2.DoDragDrop(uint index, IDataObject pDataObject, uint grfKeyState, ref uint pdwEffect)
        {
            return VSConstants.E_NOTIMPL;
        }
        int IVsSimpleObjectList2.DoRename(uint index, string pszNewName, uint grfFlags)
        {
            return VSConstants.E_NOTIMPL;
        }
        int IVsSimpleObjectList2.EnumClipboardFormats(uint index, uint grfFlags, uint celt, VSOBJCLIPFORMAT[] rgcfFormats, uint[] pcActual)
        {
            return VSConstants.E_NOTIMPL;
        }
        int IVsSimpleObjectList2.FillDescription2(uint index, uint grfOptions, IVsObjectBrowserDescription3 pobDesc)
        {
            if (index >= _items.Count)
                return VSConstants.E_INVALIDARG;

            return _items[(int)index].FillDescription(grfOptions, pobDesc);
        }
        int IVsSimpleObjectList2.GetBrowseObject(uint index, out object ppdispBrowseObj)
        {
            ppdispBrowseObj = null;
            return VSConstants.E_NOTIMPL;
        }
        int IVsSimpleObjectList2.GetCapabilities2(out uint pgrfCapabilities)
        {
            pgrfCapabilities = 0;
            return VSConstants.E_NOTIMPL;
        }
        int IVsSimpleObjectList2.GetCategoryField2(uint index, int category, out uint pfCatField)
        {
            if ((int)index == -1) {
                pfCatField = 0;
                return VSConstants.S_OK;
            }

            if (index >= _items.Count) {
                pfCatField = 0;
                return VSConstants.E_INVALIDARG;
            }

            return _items[(int)index].GetCategoryField(category, out pfCatField);
        }
        int IVsSimpleObjectList2.GetClipboardFormat(uint index, uint grfFlags, FORMATETC[] pFormatetc, STGMEDIUM[] pMedium)
        {
            return VSConstants.E_NOTIMPL;
        }
        int IVsSimpleObjectList2.GetContextMenu(uint index, out Guid pclsidActive, out int pnMenuId, out IOleCommandTarget ppCmdTrgtActive)
        {
            if (index >= _items.Count) {
                pclsidActive    = Guid.Empty;
                pnMenuId        = 0;
                ppCmdTrgtActive = null;
                return VSConstants.E_INVALIDARG;
            }

            return _items[(int)index].GetContextMenu(out pclsidActive, out pnMenuId, out ppCmdTrgtActive);
        }
        int IVsSimpleObjectList2.GetDisplayData(uint index, VSTREEDISPLAYDATA[] pData)
        {
            if (index >= _items.Count)
                return VSConstants.E_INVALIDARG;

            return _items[(int)index].GetDisplayData(pData);
        }
        int IVsSimpleObjectList2.GetExpandable3(uint index, uint listTypeExcluded, out int pfExpandable)
        {
            if (index >= _items.Count) {
                pfExpandable = 0;
                return VSConstants.E_INVALIDARG;
            }

            pfExpandable = _items[(int)index].GetExpandable3(listTypeExcluded) ? 1 : 0;
            return VSConstants.S_OK;
        }
        int IVsSimpleObjectList2.GetExtendedClipboardVariant(uint index, uint grfFlags, VSOBJCLIPFORMAT[] pcfFormat, out object pvarFormat)
        {
            pvarFormat = null;
            return VSConstants.E_NOTIMPL;
        }
        int IVsSimpleObjectList2.GetFlags(out uint pFlags)
        {
            pFlags = 0;
            return VSConstants.E_NOTIMPL;
        }
        int IVsSimpleObjectList2.GetItemCount(out uint pCount)
        {
            pCount = (uint)_items.Count;
            return VSConstants.S_OK;
        }
        int IVsSimpleObjectList2.GetList2(uint index, uint listType, uint flags, VSOBSEARCHCRITERIA2[] pobSrch, out IVsSimpleObjectList2 ppIVsSimpleObjectList2)
        {
            if (index >= _items.Count) {
                ppIVsSimpleObjectList2 = null;
                return VSConstants.E_INVALIDARG;
            }

            return _items[(int)index].GetList(listType, flags, pobSrch, out ppIVsSimpleObjectList2);
        }
        int IVsSimpleObjectList2.GetMultipleSourceItems(uint index, uint grfGSI, uint cItems, VSITEMSELECTION[] rgItemSel)
        {
            return VSConstants.E_NOTIMPL;
        }
        int IVsSimpleObjectList2.GetNavInfo(uint index, out IVsNavInfo ppNavInfo)
        {
            ppNavInfo = null;
            return VSConstants.E_NOTIMPL;
        }
        int IVsSimpleObjectList2.GetNavInfoNode(uint index, out IVsNavInfoNode ppNavInfoNode)
        {
            if (index >= _items.Count) {
                ppNavInfoNode = null;
                return VSConstants.E_INVALIDARG;
            }

            ThreadHelper.ThrowIfNotOnUIThread();
            ppNavInfoNode = _items[(int)index] as IVsNavInfoNode;

            return (ppNavInfoNode != null) ? VSConstants.S_OK : VSConstants.E_NOTIMPL;
        }
        int IVsSimpleObjectList2.GetProperty(uint index, int propid, out object pvar)
        {
            pvar = null;
            return VSConstants.E_NOTIMPL;
        }
        int IVsSimpleObjectList2.GetSourceContextWithOwnership(uint index, out string pbstrFilename, out uint pulLineNum)
        {
            pbstrFilename = null;
            pulLineNum = 0;
            return VSConstants.E_NOTIMPL;
        }
        int IVsSimpleObjectList2.GetTextWithOwnership(uint index, VSTREETEXTOPTIONS tto, out string pbstrText)
        {
            if (index >= _items.Count) {
                pbstrText = null;
                return VSConstants.E_INVALIDARG;
            }

            pbstrText = _items[(int)index].GetText(tto);

            return pbstrText != null ? VSConstants.S_OK : VSConstants.E_NOTIMPL;
        }
        int IVsSimpleObjectList2.GetTipTextWithOwnership(uint index, VSTREETOOLTIPTYPE eTipType, out string pbstrText)
        {
            if (index >= _items.Count) {
                pbstrText = null;
                return VSConstants.E_INVALIDARG;
            }

            pbstrText = _items[(int)index].GetTipText(eTipType);

            return pbstrText != null ? VSConstants.S_OK : VSConstants.E_NOTIMPL;
        }
        int IVsSimpleObjectList2.GetUserContext(uint index, out object ppunkUserCtx)
        {
            ppunkUserCtx = null;
            return VSConstants.E_NOTIMPL;
        }
        int IVsSimpleObjectList2.GoToSource(uint index, VSOBJGOTOSRCTYPE srcType)
        {
            if (index >= _items.Count)
                return VSConstants.E_INVALIDARG;

            return _items[(int)index].GoToSource(srcType);
        }
        int IVsSimpleObjectList2.LocateNavInfoNode(IVsNavInfoNode pNavInfoNode, out uint pulIndex)
        {
            pulIndex = 0;
            return VSConstants.E_NOTIMPL;
        }
        int IVsSimpleObjectList2.OnClose(VSTREECLOSEACTIONS[] ptca)
        {
            return VSConstants.E_NOTIMPL;
        }
        int IVsSimpleObjectList2.QueryDragDrop(uint index, IDataObject pDataObject, uint grfKeyState, ref uint pdwEffect)
        {
            return VSConstants.E_NOTIMPL;
        }
        int IVsSimpleObjectList2.ShowHelp(uint index)
        {
            return VSConstants.E_NOTIMPL;
        }
        int IVsSimpleObjectList2.UpdateCounter(out uint pCurUpdate)
        {
            pCurUpdate = 0;
            return VSConstants.S_OK;
        }
    }
}
