using System.Collections.Generic;
using System.Text;

namespace Wpf2Html5.Utility
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
    }
}
