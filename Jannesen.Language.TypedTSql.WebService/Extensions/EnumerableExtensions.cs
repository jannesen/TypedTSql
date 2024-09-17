using System.Collections.Generic;

namespace Jannesen.Language.TypedTSql.WebService.Extensions
{
    public static class EnumerableExtensions
    {
        public static bool EqualItems<T>(IReadOnlyList<T> left, IReadOnlyList<T> right)
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
        public static bool EqualItems<T>(IReadOnlyCollection<T> left, IReadOnlyCollection<T> right)
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
        public static bool EqualItems<TKey, TValue>(IReadOnlyDictionary<TKey, TValue> left, IReadOnlyDictionary<TKey, TValue> right)
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

        public static int GetItemsHashCode<T>(this IReadOnlyCollection<T> list)
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
        public static int GetItemsHashCode<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> list)
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
