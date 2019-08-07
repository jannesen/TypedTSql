using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace Jannesen.Language.TypedTSql.Library
{
    [DebuggerTypeProxy(typeof(ICollectionDebugView<>))]
    [DebuggerDisplay("Count = {Count}")]
    public abstract class ListHash<TItem,TKey>: IList<TItem>, IReadOnlyList<TItem>
                                                where TKey: IEquatable<TKey>
    {
        public delegate         bool    whenFilter(TItem item);

        private enum AddMode
        {
            TryAdd,
            Add,
            Update
        }
        private struct Bucket
        {
            public      TItem       item;
            public      int         next;
            public      int         hashcode;
        }

        private                 Bucket[]                            _buckets;
        private                 int[]                               _hashtable;
        private                 int                                 _size;
        private                 int                                 _version;

        public                                                      ListHash(int capacity)
        {
            _resize(capacity, true);
        }
        public                                                      ListHash(IList<TItem> list)
        {
            _resize(list.Count, true);

            for (int i = 0 ; i < list.Count ; ++i)
                _addUpdate(list[i], AddMode.Add);

        }
        public                  int                                 Count
        {
            get {
                return _size;
            }
        }

        public                  bool                                IsReadOnly
        {
            get { return false; }
        }
                                bool                                ICollection<TItem>.IsReadOnly
        {
            get { return false; }
        }

        public                  TItem                               this[int index]
        {
            get {
                if (index<0 || (uint)index >= (uint)_size)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return _buckets[index].item;
            }
            set {
                throw new NotSupportedException("ListHash.set_this not supported.");
            }
        }

        public                  ReadOnlyCollection<TItem>           AsReadOnly()
        {
            return new ReadOnlyCollection<TItem>(this);
        }

        public                  TItem                               GetValue(TKey key)
        {
            if (!TryGetValue(key, out TItem item))
                throw new KeyNotFoundException("Can't find '" + key.ToString() + "' in ListHash.");

            return item;
        }
        public                  bool                                TryGetValue(TKey key, out TItem item)
        {
            key = NormalizeKey(key);

            int     hashcode = key.GetHashCode();
            int     idx      = _hashtable[_hashindex(hashcode)];

            while (idx >= 0) {
                if (_buckets[idx].hashcode == hashcode && NormalizeKey(ItemKey(_buckets[idx].item)).Equals(key)) {
                    item = _buckets[idx].item;
                    return true;
                }

                idx = _buckets[idx].next;
            }

            item = default;
            return false;
        }
        public                  bool                                Contains(TItem item)
        {
            return _size != 0 && IndexOf(item) != -1;
        }
        public                  bool                                Contains(TKey key)
        {
            return _size != 0 && IndexOf(key) != -1;
        }

        public                  int                                 IndexOf(TItem item)
        {
            return IndexOf(item, 0, _size);
        }
        public                  int                                 IndexOf(TKey key)
        {
            key = NormalizeKey(key);

            int     hashcode = key.GetHashCode();
            int     idx      = _hashtable[_hashindex(hashcode)];

            while (idx >= 0) {
                if (_buckets[idx].hashcode == hashcode && NormalizeKey(ItemKey(_buckets[idx].item)).Equals(key))
                    return idx;

                idx = _buckets[idx].next;
            }

            return -1;
        }
        public                  int                                 IndexOf(TItem item, int index)
        {
            if (index > _size)
                throw new ArgumentOutOfRangeException(nameof(index), index, nameof(index) + " out of range.");

            return IndexOf(item, index, _size-index);
        }
        public                  int                                 IndexOf(TItem item, int index, int count)
        {
            if (index > _size)
                throw new ArgumentOutOfRangeException(nameof(index), index, nameof(index) + " out of range.");

            if (count < 0 || index > _size - count)
                throw new ArgumentOutOfRangeException(nameof(count), count, nameof(count) + " out of range.");

            while (count > 0) {
                if (_buckets[index].item.Equals(item))
                    return index;
            }

            return -1;
        }

        public                  void                                Clear()
        {
            if (_size > 0) {
                Array.Clear(_buckets, 0, _size);

                for (int i=0; i<_hashtable.Length ; ++i)
                    _hashtable[i] = -1;

                _size = 0;
            }

            _version++;
        }
        public                  void                                Add(TItem item)
        {
            _addUpdate(item, AddMode.Add);
        }
        public                  bool                                TryAdd(TItem item)
        {
            return _addUpdate(item, AddMode.TryAdd);
        }
        public                  void                                AddRange(IEnumerable<TItem> collection)
        {
            if (collection != null) {
                foreach(TItem item in collection)
                    _addUpdate(item, AddMode.Add);
            }
        }
        public                  void                                Update(TItem item)
        {
            _addUpdate(item, AddMode.Update);
        }
        public                  void                                Insert(int index, TItem item)
        {
            throw new NotSupportedException("ListHash.Insert not supported.");
        }
        public                  void                                InsertRange(int index, IEnumerable<TItem> collection)
        {
            throw new NotSupportedException("ListHash.InsertRange not supported.");
        }
        public                  bool                                Remove(TItem item)
        {
            throw new NotSupportedException("ListHash.Remove not supported.");
        }
        public                  void                                RemoveAt(int index)
        {
            throw new NotSupportedException("ListHash.RemoveAt not supported.");
        }
        public                  void                                RemoveWhen(whenFilter filter)
        {
            int     p = 0;

            for(int i = 0 ; i < _size ; ++i) {
                if (!filter(_buckets[i].item)) {
                    if (p < i) {
                        _buckets[p].item     = _buckets[i].item;
                        _buckets[p].hashcode = _buckets[i].hashcode;
                    }

                    ++p;
                }
            }

            _size = p;

            for (int i=0; i<_hashtable.Length ; ++i)
                _hashtable[i] = -1;

            for (int i=0 ; i<_size ; ++i) {
                int hashidex = _hashindex(_buckets[i].hashcode);
                _buckets[i].next = _hashtable[hashidex];
                _hashtable[hashidex] = i;
            }

#if DEBUG
            _check();
#endif
        }

        public                  void                                CopyTo(TItem[] array)
        {
            CopyTo(array, 0);
        }
        public                  void                                CopyTo(TItem[] array, int arrayIndex)
        {
            if (array.Length < arrayIndex+_size)
                throw new ArgumentOutOfRangeException(nameof(arrayIndex), arrayIndex, nameof(arrayIndex) + " out of range.");

            for (int i=0 ; i<_size ; ++i)
                array[arrayIndex++] = _buckets[i].item;
        }
        public                  TItem[]                             ToArray()
        {
            TItem[] array = new TItem[_size];

            for (int i=0 ; i<_size ; ++i)
                array[i]=_buckets[i].item;

            return array;
        }

        public                  void                                OptimizeSize()
        {
            if (_size < (_buckets.Length - _buckets.Length / 10))
                _resize(_size, true);
        }

        public                  Enumerator                          GetEnumerator()
        {
            return new Enumerator(this);
        }
                                IEnumerator<TItem>                  IEnumerable<TItem>.GetEnumerator()
        {
            return new Enumerator(this);
        }
                                System.Collections.IEnumerator      System.Collections.IEnumerable.GetEnumerator()
        {
            return new Enumerator(this);
        }

        public      static      bool                                operator == (ListHash<TItem,TKey> n1, ListHash<TItem,TKey> n2)
        {
            if (object.ReferenceEquals(n1, n2))
                return true;

            if (n1 is null || n2 is null)
                return false;

            if (n1.Count != n2.Count)
                return false;

            for (int i = 0 ; i < n1.Count ; ++i) {
                var i1 = n1[i];
                var i2 = n2[i];

                if (!(i1.GetType() == i2.GetType() && i1.Equals(i2)))
                    return false;
            }

            return true;
        }
        public      static      bool                                operator != (ListHash<TItem,TKey> n1, ListHash<TItem,TKey> n2)
        {
            return !(n1 == n2);
        }
        public      override    int                                 GetHashCode()
        {
            int rtn = 0;

            foreach(var x in this)
                rtn ^= x.GetHashCode();

            return rtn;
        }
        public      override    bool                                Equals(object obj)
        {
            if (obj is ListHash<TItem,TKey>)
                return this == ((ListHash<TItem,TKey>)obj);

            return false;
        }

        protected   abstract    TKey                                ItemKey(TItem item);
        protected   virtual     TKey                                NormalizeKey(TKey key)
        {
            return key;
        }

        private                 bool                                _addUpdate(TItem item, AddMode mode)
        {
            TKey    key      = NormalizeKey(ItemKey(item));
            int     hashcode = key.GetHashCode();
            int     hashidx  = _hashindex(hashcode);
            int     idx      = _hashtable[hashidx];

            while (idx >= 0) {
                if (_buckets[idx].hashcode == hashcode && NormalizeKey(ItemKey(_buckets[idx].item)).Equals(key)) {
                    if (mode == AddMode.Add)
                        throw new ArgumentException("Item already exists in collection.");

                    if (mode == AddMode.Update)
                        _buckets[idx].item = item;

                    return false;
                }

                idx = _buckets[idx].next;
            }

            idx = _size;

            if (idx >= _buckets.Length) {
                _resize(_buckets.Length * 2, false);
                hashidx  = _hashindex(hashcode);
            }

            _size++;
            _version++;

            _buckets[idx].item     = item;
            _buckets[idx].hashcode = hashcode;
            _buckets[idx].next     = _hashtable[hashidx];
            _hashtable[hashidx] = idx;
#if DEBUG
            _check();
#endif
            return true;
        }
        private                 void                                _resize(int newsize, bool forcehash)
        {
            var buckets = new Bucket[newsize];

            if (_buckets != null)
                Array.Copy(_buckets, 0, buckets, 0, _size);

            _buckets = buckets;

            if (forcehash || _size / 4 + 4 < _hashtable.Length || _size * 4 > _hashtable.Length) {
                int hashtablesize = _hashtablesize(newsize * 2);

                if (_hashtable == null || _hashtable.Length != hashtablesize) {
                    _hashtable = new int[hashtablesize];

                    for (int i=0; i<hashtablesize ; ++i)
                        _hashtable[i] = -1;

                    for (int i=0 ; i<_size ; ++i) {
                        int hashidex = _hashindex(_buckets[i].hashcode);
                        _buckets[i].next = _hashtable[hashidex];
                        _hashtable[hashidex] = i;
                    }
                }
            }
#if DEBUG
            _check();
#endif
        }
        private                 int                                 _hashindex(int hashcode)
        {
            return (int)((uint)hashcode % _hashtable.Length);
        }
#if DEBUG
        private                 void                                _check()
        {
            for (int i = 0 ; i < _size ; ++i)
                _checkItem(i, _buckets[i].item);
        }
        private                 void                                _checkItem(int i, TItem item)
        {
            TKey    key      = NormalizeKey(ItemKey(item));
            int     hashcode = key.GetHashCode();

            Debug.Assert(_buckets[i].hashcode == hashcode);

            int     idx     = _hashtable[_hashindex(hashcode)];

            while (idx != -1) {
                if (_buckets[idx].hashcode == hashcode && Object.Equals(_buckets[idx].item, item))
                    return;

                idx = _buckets[idx].next;
            }

            Debug.Assert(false, "Can't find item in ListHash");
        }
#endif
        public struct Enumerator: IEnumerator<TItem>, System.Collections.IEnumerator
        {
            private         ListHash<TItem,TKey>    _list;
            private         int                     _index;
            private         int                     _version;
            private         TItem                   _current;

            internal                                Enumerator(ListHash<TItem,TKey> list)
            {
                this._list = list;
                _index = 0;
                _version = list._version;
                _current = default;
            }
            public          void                    Dispose()
            {
            }

            public          bool                    MoveNext()
            {
                if (_version == _list._version && ((uint)_index < (uint)_list._size)) {
                    _current = _list._buckets[_index].item;
                    _index++;
                    return true;
                }

                if (_version != _list._version)
                    throw new InvalidOperationException("Collection changed.");

                _index = _list._size + 1;
                _current = default;

                return false;
            }
            public          TItem                   Current
            {
                get {
                    return _current;
                }
            }

                            object                  System.Collections.IEnumerator.Current
            {
                get {
                    if (_index == 0 || _index >= _list._size + 1)
                        throw new InvalidOperationException("Invalid state.");

                    return Current;
                }
            }
                            void                    System.Collections.IEnumerator.Reset()
            {
                if (_version != _list._version)
                    throw new InvalidOperationException("Collection changed.");

                _index = 0;
                _current = default;
            }
        }

        private     static  int                     _hashtablesize(int n)
        {
            for (int i = 0 ; i < _primes.Length ; ++i) {
                if (_primes[i] > n)
                    return _primes[i];
            }

            return _primes[_primes.Length - 1];
        }

/*
function isPrime(value)
{
    for(var i = 2; i < value; i++) {
        if(value % i === 0) {
            return false;
        }
    }

    return value > 1;
}


var n = 3;

console.log('' + n);

while (n < 64000)
{
    var n = Math.floor((n + 2) * 1.30);

    while (!isPrime(n))
        ++n;

    console.log('' + n);
}
*/
        private     static  int[]                   _primes = new int[] {3,7,11,17,23,31,41,53,71,97,127,163,211,269,347,439,557,701,881,1103,1381,1733,2179,2729,3413,4271,5347,6689,8363,10457,13093,16369,20477,25601,32003,40009,50021,62533 };
    }

    public abstract class ListHashName<TItem>: ListHash<TItem, string>
    {
        public                                                      ListHashName(int capacity): base(capacity)
        {
        }
        public                                                      ListHashName(IList<TItem> list): base(list)
        {
        }

        protected   override    string                              NormalizeKey(string key)
        {
            return key.ToUpperInvariant();
        }
    }
}
