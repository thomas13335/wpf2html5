using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Xml;

namespace Wpf2Html5.Converter.Framework
{
    class RunConverter : FrameworkContentElementConverter<Run>
    {
        public override string HtmlTag
        {
            get { return "SPAN"; }
        }

        protected override void ConvertChildren()
        {
            base.ConvertChildren();
            Writer.WriteString(Control.Text);
        }
    }
}
