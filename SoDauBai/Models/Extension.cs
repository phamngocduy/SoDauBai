using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Data;
using System.Web.Mvc;
using System.Security.Principal;
using System.Text;

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
            new TimeSpan(20, 30, 0), // Tiet 16
            new TimeSpan(21, 15, 0), // Tiet 17
            new TimeSpan(22, 0, 0) // Tiet 18
        };

        public static string HocKy = "HocKy";
    }

    public class EMAILS
    {
        public const int GhiSo = 0;
        public const int DayBu = 1;
        public const int HoTro = 2;
    }

    public class CONFIG
    {
        public const string KHOA_SO = "KHOA_SO";
        public const string ACDM511 = "ACDM511";
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

        public static double AverageOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
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

        public static List<TSource> Concat<TSource>(this List<List<TSource>> group)
        {
            var list = new List<TSource>();
            foreach (var item in group)
                list = list.Concat(item).ToList();
            return list;
        }

        public static void ForEach<T>(this IEnumerable<T> list, Action<T> action)
        {
            foreach (var item in list)
                action(item);
        }

        public static int Count<TSource>(this IEnumerable<TSource> group, Func<TSource, int> count)
        {
            int sum = 0;
            group.ForEach(g => sum += count(g));
            return sum;
        }

        public static bool IsNullOrEmptyOrWhiteSpace(this string text)
        {
            return String.IsNullOrEmpty(text) || String.IsNullOrWhiteSpace(text);
        }

        public static void AddModelErrorFor<TValue>(this ModelStateDictionary modelState, Expression<Func<object, TValue>> property, string errorMessage)
        {
            var expression = (MemberExpression)property.Body;
            modelState.AddModelError(expression.Member.Name, errorMessage);
        }

        public static bool IsInRoles(this IPrincipal user, string roles)
        {
            foreach (var role in roles.Split(','))
                if (user.IsInRole(role))
                    return true;
            return false;
        }

        public static List<TSource> Merge<TSource>(this List<List<TSource>> source)
        {
            var list = new List<TSource>();
            foreach (var item in source)
                list.AddRange(item);
            return list;
        }

        public static byte GetHocKy(this Controller controller, SoDauBaiEntities db)
        {
            return (byte)(controller.Session[CONST.HocKy] ?? db.ThoiKhoaBieux.MaxOrDefault(tkb => tkb.HocKy));
        }

        public static T Parse<T>(this string text, string errorMessage) where T : IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
        {
            try
            {
                return (T)typeof(T).GetMethod("Parse", new Type[] { typeof(string) })
                                   .Invoke(null, new object[] { text });
            }
            catch (Exception)
            {
                throw new Exception(errorMessage);
            }
        }

        public static T Parse<T>(this string text, T defaultValue) where T : IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
        {
            try
            {
                return (T)typeof(T).GetMethod("Parse", new Type[] { typeof(string) })
                                   .Invoke(null, new object[] { text });
            }
            catch (Exception)
            {
                return defaultValue;
            }
        }

        public static string ToBase64(this string s)
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(s));
        }
    }
}