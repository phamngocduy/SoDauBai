using System;
using System.Linq;
using System.Collections.Generic;

namespace SoDauBai.Models
{
    public static class Extension
    {
        public static void For<T>(this IEnumerable<T> list, int from, int to, Action<int> action)
        {
            for (int i = from; i <= to; i++)
                action(i);
        }

        public static T Init<T>(this T obj)
        {
            return obj != null ? obj : Activator.CreateInstance<T>();
        }

        public static double AverageOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
        {
            return source.Count() > 0 ? source.Average(selector) : 0;
        }
    }
}