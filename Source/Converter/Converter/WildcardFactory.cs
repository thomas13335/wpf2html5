using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Wpf2Html5.Converter
{
    /// <summary>
    /// Converts file name masks into regular expression.
    /// </summary>
    public static class WildcardFactory
    {
        public static Regex BuildWildcards(IEnumerable<string> wildcards)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("^(");
            bool first = true;
            foreach (string w in wildcards)
            {
                if (!first) sb.Append('|'); else first = false;
                foreach (char c in w)
                {
                    if (c == '*') { sb.Append(".*"); }
                    else if (c == '?') { sb.Append("."); }
                    else
                    {
                        sb.Append(Regex.Escape(c.ToString()));
                    }
                }
            }

            sb.Append(")$");
            return new Regex(sb.ToString());
        }

        public static Regex BuildWildcardsFromList(string list)
        {
            return BuildWildcards(list.Split(';', ' ')
                .Select(u => u.Trim())
                .Where(u => u.Length > 0));
        }

        public static Regex BuildWildcards(string wildcard)
        {
            return BuildWildcards(new string[] { wildcard });
        }
    }
}
