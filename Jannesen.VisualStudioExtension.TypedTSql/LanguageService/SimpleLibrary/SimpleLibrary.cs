using System;
using System.Collections.Generic;
using Microsoft.VisualStudio;
using OLE = Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Jannesen.VisualStudioExtension.TypedTSql.Library;
using LTTS                 = Jannesen.Language.TypedTSql;

namespace Jannesen.VisualStudioExtension.TypedTSql.LanguageService.Library
{
    internal class SimpleLibrary: IVsSimpleLibrary2
    {
        public static readonly          Guid                    GUID = new Guid("04a89761-9931-443c-81bf-9316427728c5");

        public                          void                    SearchReferences(IServiceProvider serviceProvider, IVsProject project, LTTS.SymbolReferenceList referenceList)
        {
            var     objects = new List<SimpleObject>();

            foreach (var r in referenceList)
                objects.Add(new SimpleObjectSymbolReference(project, r));

            _presentNavInfo(serviceProvider, "typed-TSql references", new NavInfo(new SimpleObjectList(objects)));
        }

        private                         void                    _presentNavInfo(IServiceProvider serviceProvider, string title, NavInfo navInfo)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            ErrorHandler.ThrowOnFailure(serviceProvider.GetService<IVsFindSymbol>(typeof(SVsObjectSearch))
                                        .DoSearch(GUID, new[] {
                                                new VSOBSEARCHCRITERIA2()
                                                                    {
                                                                        dwCustom = 0,
                                                                        eSrchType = VSOBSEARCHTYPE.SO_ENTIREWORD,
                                                                        grfOptions = (uint)_VSOBSEARCHOPTIONS2.VSOBSO_LISTREFERENCES | (uint)_VSOBSEARCHOPTIONS.VSOBSO_CASESENSITIVE,
                                                                        pIVsNavInfo = navInfo,
                                                                        szName = title,
                                                                    }
                                            }));
        }

        private                         uint                    _getSupportedCategoryFields(uint category)
        {
            switch (category) {
            case (uint)LIB_CATEGORY.LC_LISTTYPE:
                return (uint)_LIB_LISTTYPE.LLT_HIERARCHY;
            }

            return 0;
        }
        private                         IVsSimpleObjectList2    _getList(uint listType, uint flags, VSOBSEARCHCRITERIA2[] pobSrch)
        {
            if (listType != (uint)_LIB_LISTTYPE.LLT_HIERARCHY)
                throw new NotSupportedException("Invalid listType.");

            if ((flags & (uint)_LIB_LISTFLAGS.LLF_USESEARCHFILTER) == 0)
                throw new NotSupportedException("invalid flags.");

            if (pobSrch == null ||
                pobSrch.Length != 1 ||
                (pobSrch[0].grfOptions & (uint)_VSOBSEARCHOPTIONS2.VSOBSO_LISTREFERENCES) == 0 ||
                !(pobSrch[0].pIVsNavInfo is NavInfo))
                throw new NotSupportedException("invalid search.");

            return ((NavInfo)pobSrch[0].pIVsNavInfo).GetObjectList();
        }

                                        int                     IVsSimpleLibrary2.AddBrowseContainer(VSCOMPONENTSELECTORDATA[] pcdComponent, ref uint pgrfOptions, out string pbstrComponentAdded)
        {
            pbstrComponentAdded = null;
            return VSConstants.E_NOTIMPL;
        }
                                        int                     IVsSimpleLibrary2.CreateNavInfo(SYMBOL_DESCRIPTION_NODE[] rgSymbolNodes, uint ulcNodes, out IVsNavInfo ppNavInfo)
        {
            ppNavInfo = null;
            return VSConstants.E_NOTIMPL;
        }
                                        int                     IVsSimpleLibrary2.GetBrowseContainersForHierarchy(IVsHierarchy pHierarchy, uint celt, VSBROWSECONTAINER[] rgBrowseContainers, uint[] pcActual)
        {
            throw new NotImplementedException();
        }
                                        int                     IVsSimpleLibrary2.GetGuid(out Guid pguidLib)
        {
            pguidLib = GUID;
            return VSConstants.S_OK;
        }
                                        int                     IVsSimpleLibrary2.GetLibFlags2(out uint pgrfFlags)
        {
            pgrfFlags = (uint)_LIB_FLAGS2.LF_SUPPORTSLISTREFERENCES;
            return VSConstants.S_OK;
        }
                                        int                     IVsSimpleLibrary2.GetList2(uint listType, uint flags, VSOBSEARCHCRITERIA2[] pobSrch, out IVsSimpleObjectList2 ppIVsSimpleObjectList2)
        {
            try {
                ppIVsSimpleObjectList2 = _getList(listType, flags, pobSrch);
            }
            catch(Exception err) {
                System.Diagnostics.Debug.WriteLine("Jannesen.VisualStudioExtensions.TypedTSql.Library.Library.GetList2(0x" + listType.ToString("X") + ", 0x" + flags.ToString("X") +"): " + err.Message);
                ppIVsSimpleObjectList2 = null;
            }

            return ppIVsSimpleObjectList2 != null
                ? VSConstants.S_OK
                : VSConstants.E_FAIL;
        }
                                        int                     IVsSimpleLibrary2.GetSeparatorStringWithOwnership(out string pbstrSeparator)
        {
            pbstrSeparator = ".";
            return VSConstants.S_OK;
        }
                                        int                     IVsSimpleLibrary2.GetSupportedCategoryFields2(int category, out uint pgrfCatField)
        {
            pgrfCatField = _getSupportedCategoryFields((uint)category);
            return VSConstants.S_OK;
        }
                                        int                     IVsSimpleLibrary2.LoadState(OLE.IStream pIStream, LIB_PERSISTTYPE lptType)
        {
            return VSConstants.E_NOTIMPL;
        }
                                        int                     IVsSimpleLibrary2.RemoveBrowseContainer(uint dwReserved, string pszLibName)
        {
            return VSConstants.E_NOTIMPL;
        }
                                        int                     IVsSimpleLibrary2.SaveState(OLE.IStream pIStream, LIB_PERSISTTYPE lptType)
        {
            return VSConstants.E_NOTIMPL;
        }
                                        int                     IVsSimpleLibrary2.UpdateCounter(out uint pCurUpdate)
        {
            pCurUpdate = 0;
            return VSConstants.E_NOTIMPL;
        }
    }
}
