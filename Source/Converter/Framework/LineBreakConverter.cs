using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Wpf2Html5.Converter.Framework
{
    class LineBreakConverter : FrameworkContentElementConverter<LineBreak>
    {
        public override string HtmlTag
        {
            get { return "BR"; }
        }
    }
}
