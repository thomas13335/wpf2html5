using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf2Html5.Exceptions
{
    public class RewriterException : ConverterException
    {
        public Location Location { get; private set; }

        public RewriterException(Location location, ErrorCode code, string msg)
            : base(code, msg)
        {
            Location = location;
        }

        public RewriterException(SyntaxNode node, ErrorCode code, string msg)
            : this(node.GetLocation(), code, msg)
        {
        }
    }
}
