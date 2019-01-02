using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;

namespace Jannesen.VisualStudioExtension.TypedTSql.LanguageService.Library
{
    internal class NavInfo: IVsNavInfo
    {
        private             IVsSimpleObjectList2        _objectList;

        public              NavInfo(IVsSimpleObjectList2 objectList)
        {
            _objectList = objectList;
        }

        public               IVsSimpleObjectList2       GetObjectList()
        {
            return _objectList;
        }

                            int                         IVsNavInfo.EnumCanonicalNodes(out IVsEnumNavInfoNodes ppEnum)
                            {
                                ppEnum = null;
                                return VSConstants.E_NOTIMPL;
                            }
                            int                         IVsNavInfo.EnumPresentationNodes(uint dwFlags, out IVsEnumNavInfoNodes ppEnum)
                            {
                                ppEnum = null;
                                return VSConstants.E_NOTIMPL;
                            }
                            int                         IVsNavInfo.GetLibGuid(out Guid pGuid)
                            {
                                pGuid = Guid.Empty;
                                return VSConstants.E_NOTIMPL;
                            }
                            int                         IVsNavInfo.GetSymbolType(out uint pdwType)
                            {
                                pdwType = 0;
                                return VSConstants.E_NOTIMPL;
                            }
    }
}
