using System;
using System.Collections.Generic;
using System.Linq;

namespace WeavR.Tasks
{
    public static class Extensions
    {
        public static IEnumerable<TSource> IgnoreNull<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null) return new TSource[0];
            return source;
        }
    }
}