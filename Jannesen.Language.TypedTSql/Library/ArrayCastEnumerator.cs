using System;
using System.Collections;
using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql.Library
{
    public struct ArrayCastEnumerator<T>: IEnumerator<T>
    {
        private readonly    object[]            _array;
        private int                             _index;
 
        public      T                           Current             => (T)_array[_index];
                    object                      IEnumerator.Current => _array[_index];

        internal                                ArrayCastEnumerator(object[] array)
        {
            _array = array;
            _index = -1;
        }
        public      void                        Dispose()
        {
        }
 
        public      bool                        MoveNext()
        {
            var  index  = _index + 1;
            var  length = _array.Length;

            if (index >= length) {
                _index = length;
                return false;
            }
            else {
                _index = index;
                return true;
            }
        }
        public      void                        Reset()
        {
            _index = -1;
        }
    }
}
