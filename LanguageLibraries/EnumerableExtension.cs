using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageLibrary
{
    public static class EnumerableExtension
    {
        public static IEnumerable<TSource> ToEnumerable<TSource>(this TSource value)
        {
            yield return value;
        }

        public static IEnumerable<(int Index, TSource Element)> Indexed<TSource>(this IEnumerable<TSource> source)
            => source.Select((element, index) => (index, element));

        public static IEnumerable<IEnumerable<TSource>> GetPermutation<TSource>(this IEnumerable<TSource> source)
            where TSource : IEquatable<TSource>
        {
            return GetPermutation(source.GroupBy(arg => arg).ToDictionary(group => group.Key, group => group.Count()));
        }

        public static IEnumerable<IEnumerable<TSource>> GetPermutation<TSource>(this IDictionary<TSource, int> source)
            where TSource : IEquatable<TSource>
        {
            if (source.Keys.Count == 0)
            {
                yield return Enumerable.Empty<TSource>();
                yield break;
            }

            foreach (var item in source.Keys.ToArray())
            {
                source[item]--;

                if (source[item] == 0)
                {
                    source.Remove(item);
                }

                foreach (var innerSource in source.GetPermutation())
                {
                    yield return item.ToEnumerable().Concat(innerSource);
                }

                if (!source.ContainsKey(item))
                {
                    source[item] = 1;
                }
                else
                {
                    source[item]++;
                }
            }
        }

        public static IEnumerable<IEnumerable<TSource>> GetDuplicatePermutation<TSource>(this IEnumerable<TSource> source, int count)
            where TSource : IEquatable<TSource>
        {
            return GetDuplicatePermutation(source.ToHashSet(), count);
        }

        public static IEnumerable<IEnumerable<TSource>> GetDuplicatePermutation<TSource>(this HashSet<TSource> source, int count)
            where TSource : IEquatable<TSource>
        {
            if (count == 0)
            {
                yield return Enumerable.Empty<TSource>();
                yield break;
            }

            foreach (var item in source)
            {
                foreach (var innerSource in source.GetDuplicatePermutation(count-1))
                {
                    yield return item.ToEnumerable().Concat(innerSource);
                }
            }
        }
    }
}
