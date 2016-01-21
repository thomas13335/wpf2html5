using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wpf2Html5.Converter.Interface;

namespace Wpf2Html5
{
    static class Extensions
    {
        public static string ToSeparatorList(this IEnumerable<object> list, string sep = ", ")
        {
            var sb = new StringBuilder();
            foreach (var s in list)
            {
                if (0 < sb.Length) sb.Append(sep);
                sb.AppendFormat("{0}", s);
            }

            return sb.ToString();
        }

        public static T GetCurrent<T>(this IConverterContext context) where T : class
        {
            return context.GetCurrent(typeof(T)) as T;
        }

        public static bool IsZero(this Thickness th)
        {
            return th.Left == 0 && th.Right == 0 && th.Top == 0 && th.Bottom == 0;
        }
    }
}
