using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Data;

namespace SoDauBai.Models
{
    public class CONST
    {
        public static int[] THU =
        {
            8, // Sunday
            2, // Monday
            3, // Tuesday
            4, // Wednesday
            5, // Thrusday
            6, // Friday
            7 // Saturday
        };

        public static TimeSpan[] TIET =
        {
            new TimeSpan(0, 45, 0), // Tiet 0
            new TimeSpan(7, 0, 0), // Tiet 1
            new TimeSpan(7, 50, 0), // Tiet 2
            new TimeSpan(8, 40, 0), // Tiet 3
            new TimeSpan(9, 35, 0), // Tiet 4
            new TimeSpan(10, 25, 0), // Tiet 5
            new TimeSpan(11, 15, 0), // Tiet 6
            new TimeSpan(13, 0, 0), // Tiet 7
            new TimeSpan(13, 50, 0), // Tiet 8
            new TimeSpan(14, 40, 0), // Tiet 9
            new TimeSpan(15, 35, 0), // Tiet 10
            new TimeSpan(16, 25, 0), // Tiet 11
            new TimeSpan(17, 15, 0), // Tiet 12
            new TimeSpan(18, 5, 0), // Tiet 13
            new TimeSpan(18, 55, 0), // Tiet 14
            new TimeSpan(19, 45, 0), // Tiet 15
        };
    }

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

        public static int ToIntOrDefault(this string strValue, int defValue)
        {
            int.TryParse(strValue, out defValue);
            return defValue;
        }

        public static TResult MaxOrDefault<TSource, TResult>(this IQueryable<TSource> source, Expression<Func<TSource, TResult>> selector)
        {
            return source.Count() > 0 ? source.Max(selector) : (TResult)Activator.CreateInstance(typeof(TResult));
        }

        public static string GetText(this IDataRecord excel, int index)
        {
            var text = excel.GetValue(index) ?? "";
            return text.ToString().Trim();
        }
    }
}