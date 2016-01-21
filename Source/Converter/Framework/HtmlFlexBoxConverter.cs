using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf2Html5.StockObjects;

namespace Wpf2Html5.Converter.Framework
{
    class FlexBoxContext
    { }

    class HtmlFlexBoxConverter : PanelConverterBase<HtmlFlexBox>
    {
        public override string HtmlTag
        {
            get { return "DIV"; }
        }

        protected override void ConvertChildren()
        {
            base.ConvertChildren();

            Context.Push(new FlexBoxContext());

            // push the current orientation onto the stack ...
            int childcount = 0;
            foreach (var child in Control.Children)
            {
                Context.Convert(child, Writer);

                ++childcount;
            }

            if (0 == childcount)
            {
                Writer.WriteString("\u00a0");
            }

            Context.Pop();
        }
    }
}
