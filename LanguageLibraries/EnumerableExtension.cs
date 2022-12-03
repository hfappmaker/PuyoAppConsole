using System;
using System.Collections.Generic;
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
    }
}
