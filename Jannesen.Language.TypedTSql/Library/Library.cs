using System;

namespace Jannesen.Language.TypedTSql.Library
{
    static class Library
    {
        public  static      T[]         ArrayJoin<T>(T[] a1, T[] a2)
        {
            int l1 = (a1 != null ? a1.Length : 0);
            int l2 = (a2 != null ? a2.Length : 0);

            T[] rtn = new T[l1 + l2];

            if (l1 > 0) Array.Copy(a1, 0, rtn, 0,  l1);
            if (l2 > 0) Array.Copy(a2, 0, rtn, l1, l2);

            return rtn;
        }
    }
}
