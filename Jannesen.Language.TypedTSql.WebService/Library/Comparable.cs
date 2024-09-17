using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql.WebService.Library
{
    internal class ComparableList<T>: List<T>  
    {
        public  static      bool                       operator == (ComparableList<T> left, ComparableList<T> right)
        {
            return CompareExtensions.EqualItems(left, right);

        }
        public  static      bool                       operator != (ComparableList<T> left, ComparableList<T> right)
        {
            return !(left == right);
        }
        public  override    bool                       Equals(object obj)
        {
            return obj is ComparableList<T> other && this == other;
        }
        public  override    int                        GetHashCode()
        {
            return this.GetItemsHashCode();
        }
    }

    internal class ComparableHashSet<T>: HashSet<T>  
    {
        public  static      bool                       operator == (ComparableHashSet<T> left, ComparableHashSet<T> right)
        {
            return CompareExtensions.EqualItems(left, right);

        }
        public  static      bool                       operator != (ComparableHashSet<T> left, ComparableHashSet<T> right)
        {
            return !(left == right);
        }
        public  override    bool                       Equals(object obj)
        {
            return obj is ComparableHashSet<T> other && this == other;
        }
        public  override    int                        GetHashCode()
        {
            return this.GetItemsHashCode();
        }
    }

    internal class ComparableDictionary<TKey, TValue>: Dictionary<TKey, TValue>
    {
        public  static      bool                       operator == (ComparableDictionary<TKey, TValue> left, ComparableDictionary<TKey, TValue> right)
        {
            return CompareExtensions.EqualItems(left, right);

        }
        public  static      bool                       operator != (ComparableDictionary<TKey, TValue> left, ComparableDictionary<TKey, TValue> right)
        {
            return !(left == right);
        }
        public  override    bool                       Equals(object obj)
        {
            return obj is ComparableDictionary<TKey, TValue> other && this == other;
        }
        public  override    int                        GetHashCode()
        {
            return this.GetItemsHashCode();
        }
    }

    internal class ComparableSortedDictionary<TKey, TValue>: SortedDictionary<string, TValue>
    {
        public  static      bool                       operator == (ComparableSortedDictionary<TKey, TValue> left, ComparableSortedDictionary<TKey, TValue> right)
        {
            return CompareExtensions.EqualItems(left, right);

        }
        public  static      bool                       operator != (ComparableSortedDictionary<TKey, TValue> left, ComparableSortedDictionary<TKey, TValue> right)
        {
            return !(left == right);
        }
        public  override    bool                       Equals(object obj)
        {
            return obj is ComparableSortedDictionary<TKey, TValue> other && this == other;
        }
        public  override    int                        GetHashCode()
        {
            return this.GetItemsHashCode();
        }
    }

    public static class CompareExtensions
    {
        public static bool  EqualItems<T>(IReadOnlyList<T> left, IReadOnlyList<T> right)
        {
            if (ReferenceEquals(left, right))  return true;
            if (left == null || right == null) return false;
            if (left.Count != right.Count) return false;

            for (int i = 0 ; i < left.Count ; ++i) {
                if (!object.Equals(left[i], right[i])) {
                    return false;
                }
            }

            return true;
        }
        public static bool  EqualItems<T>(IReadOnlyCollection<T> left, IReadOnlyCollection<T> right)
        {
            if (ReferenceEquals(left, right))  return true;
            if (left == null || right == null) return false;
            if (left.Count != right.Count) return false;

            using (var eleft = left.GetEnumerator())
            {
                using (var eright = right.GetEnumerator()) {
                    while (eleft.MoveNext())
                    {
                        if (!eright.MoveNext()) {
                            return false;
                        }

                        if (!object.Equals(eleft.Current, eright.Current)) {
                            return false;
                        }
                    }

                    if (eright.MoveNext()) {
                        return false;
                    }
                }
            }

            return true;
        }
        public static bool  EqualItems<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> left, IReadOnlyDictionary<TKey, TValue> right)
        {
            if (ReferenceEquals(left, right))  return true;
            if (left == null && right == null) return true;
            if (left == null || right == null) return false;

            using (var eleft = left.GetEnumerator())
            {
                using (var eright = right.GetEnumerator()) {
                    while (eleft.MoveNext())
                    {
                        if (!eright.MoveNext()) {
                            return false;
                        }

                        if (!(object.Equals(eleft.Current.Key,   eright.Current.Key) &&
                              object.Equals(eleft.Current.Value, eright.Current.Value))) {
                            return false;
                        }
                    }

                    if (eright.MoveNext()) {
                        return false;
                    }
                }
            }

            return true;
        }

        public static int   GetItemsHashCode<T>(this IReadOnlyCollection<T> list)
        {
            unchecked {
                var hash = 19;

                if (list != null) {
                    foreach (var item in list) {
                        hash *= 31;
                        if (item != null) {
                            hash ^= item.GetHashCode();
                        }
                    }
                }

                return hash;
            }
        }
        public static int   GetItemsHashCode<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> list)
        {
            unchecked {
                var hash = 19;

                if (list != null) {
                    foreach (var item in list) {
                        hash *= 31;
                        if (item.Key != null)   hash ^= item.Key.GetHashCode();
                        if (item.Value != null) hash ^= item.Value.GetHashCode();
                    }
                }

                return hash;
            }
        }
    }
}
