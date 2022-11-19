using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LanguageLibrary
{
    public static class DelegateExtension
    {
        public static Func<T1, Func<T2, T3>> Curry<T1, T2, T3>(this Func<T1, T2, T3> func)
        {
            return arg1 => arg2 => func(arg1, arg2);
        }

        public static Func<T2, T3> Partial<T1, T2, T3>(this Func<T1, T2, T3> func, T1 value)
        {
            return func.Curry()(value);
        }
    }
}
