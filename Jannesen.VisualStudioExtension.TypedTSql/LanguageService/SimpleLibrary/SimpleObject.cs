using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell.Interop;

namespace Jannesen.VisualStudioExtension.TypedTSql.LanguageService.Library
{
    abstract class SimpleObject: IVsNavInfoNode, IOleCommandTarget
    {
        public      virtual     bool                    CanGoToSource(VSOBJGOTOSRCTYPE srcType)
        {
            return false;
        }
        public      virtual     int                     CountSourceItems(out IVsHierarchy ppHier, out uint pItemid, out uint pcItems)
        {
            ppHier = null;
            pItemid = 0;
            pcItems = 0;
            return VSConstants.E_FAIL;
        }
        public      virtual     int                     FillDescription(uint grfOptions, IVsObjectBrowserDescription3 pobDesc)
        {
            return VSConstants.E_NOTIMPL;
        }
        public      virtual     int                     GetCategoryField(int category, out uint pfCatField)
        {
            pfCatField = 0;
            return VSConstants.E_NOTIMPL;
        }
        public      virtual     int                     GetContextMenu(out Guid pclsidActive, out int pnMenuId, out IOleCommandTarget ppCmdTrgtActive)
        {
            pclsidActive    = Microsoft.VisualStudio.Shell.VsMenus.guidSHLMainMenu;
            pnMenuId        = /*IDM_VS_CTXT_CV_ITEM*/ 0x0433;
            ppCmdTrgtActive = this;
            return VSConstants.S_OK;

        }
        public      virtual     int                     GetDisplayData(VSTREEDISPLAYDATA[] pData)
        {
            return VSConstants.S_OK;
        }
        public      virtual     bool                    GetExpandable3(uint listTypeExcluded)
        {
            return false;
        }
        public      virtual     int                     GetList(uint listType, uint flags, VSOBSEARCHCRITERIA2[] pobSrch, out IVsSimpleObjectList2 ppIVsSimpleObjectList2)
        {
            ppIVsSimpleObjectList2 = null;
            return VSConstants.E_FAIL;
        }
        public      virtual     string                  GetText(VSTREETEXTOPTIONS tto)
        {
            return null;
        }
        public      virtual     string                  GetTipText(VSTREETOOLTIPTYPE eTipType)
        {
            return null;
        }
        public      virtual     int                     GoToSource(VSOBJGOTOSRCTYPE srcType)
        {
            return VSConstants.E_NOTIMPL;
        }

                                int                     IVsNavInfoNode.get_Name(out string pbstrName)
        {
            pbstrName = "todo";
            return VSConstants.S_OK;
        }
                                int                     IVsNavInfoNode.get_Type(out UInt32 pllt)
        {
            pllt = 0;
            return VSConstants.S_OK;
        }
                                int                     IOleCommandTarget.Exec(ref Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            return (int)Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_NOTSUPPORTED;
        }
                                int                     IOleCommandTarget.QueryStatus(ref Guid pguidCmdGroup, uint cCmds, OLECMD[] prgCmds, IntPtr pCmdText)
        {
            return (int)Microsoft.VisualStudio.OLE.Interop.Constants.OLECMDERR_E_NOTSUPPORTED;
        }
    }
}
