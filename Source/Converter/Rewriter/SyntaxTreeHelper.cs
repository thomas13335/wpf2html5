using Microsoft.CodeAnalysis;
using System.Linq;

namespace Wpf2Html5.Converter.Rewriter
{
    public static class SyntaxTreeHelper
    {
        public static void PrintTree(this SyntaxNode node, int depth = 0)
        {
            var prefix = new string(' ', depth * 2);
            var annotation = node.GetAnnotations("ltype").FirstOrDefault();
            var sannot = null == annotation ? string.Empty : annotation.Data;

            var head = prefix + node.GetType().Name;
            head = head.PadRight(50);
            head += sannot.PadRight(30);
            head += string.Format("{0:X8} ", node.GetHashCode());
            head += node.ToString();
            Log.Trace("{0}", head);

            foreach (var childnode in node.ChildNodes())
            {
                PrintTree(childnode, depth + 1);
            }
        }
    }
}
