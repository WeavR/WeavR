using System;
using System.Collections.Generic;
using System.Linq;

namespace WeavR
{
    public static class Extensions
    {
        public static IEnumerable<TSource> IgnoreNull<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null) return new TSource[0];
            return source;
        }

        public static IEnumerable<TSource> WhereWithActions<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate, Action<TSource> onHit = null, Action<TSource> onMiss = null)
        {
            if (source == null)
                throw new ArgumentNullException("source", "source is null.");
            if (predicate == null)
                throw new ArgumentNullException("predicate", "predicate is null.");

            return InternalWhereWithActions<TSource>(source, predicate, onHit ?? (s => { }), onMiss ?? (s => { }));
        }

        private static IEnumerable<TSource> InternalWhereWithActions<TSource>(IEnumerable<TSource> source, Func<TSource, bool> predicate, Action<TSource> onHit, Action<TSource> onMiss)
        {
            foreach (var item in source)
            {
                if (predicate(item))
                {
                    onHit(item);
                    yield return item;
                }
                else
                {
                    onMiss(item);
                }
            }
        }
    }
}