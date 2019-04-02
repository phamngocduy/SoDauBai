﻿using System;
using System.Linq;
using System.Collections.Generic;

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
            new TimeSpan(), // Tiet 0
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
            new TimeSpan(17, 15, 0) // Tiet 12
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
    }
}