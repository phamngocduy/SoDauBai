using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SoDauBai.Models
{
    public static class Extension
    {
        public static void For<T>(this IEnumerable<T> list, int from, int to, Action<int> action)
        {
            for (int i = from; i <= to; i++)
                action(i);
        }
    }
}