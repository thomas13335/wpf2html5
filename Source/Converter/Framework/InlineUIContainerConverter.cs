using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace Wpf2Html5.Converter.Framework
{
    class InlineUIContainerConverter : FrameworkContentElementConverter<InlineUIContainer>
    {
        public override string HtmlTag
        {
            get { return "SPAN"; }
        }

        protected override void ConvertChildren()
        {
            base.ConvertChildren();

            Context.Convert(Control.Child, Writer);
        }
    }
}
