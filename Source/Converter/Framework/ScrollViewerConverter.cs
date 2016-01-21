using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Wpf2Html5.Converter.Framework
{
    class ScrollViewerConverter : FrameworkElementConverter<ScrollViewer>
    {
        public override string HtmlTag
        {
            get
            {
                return "DIV";
            }
        }

        protected override void MapCommonProperties()
        {
            base.MapCommonProperties();

            if(!MapProperty(ScrollViewer.VerticalScrollBarVisibilityProperty))
            {
                var visiblity = Control.VerticalScrollBarVisibility;

                switch (visiblity)
                {
                    case ScrollBarVisibility.Auto:
                        Style.Add("overflow-y", "");
                        break;
                }
            }
        }

        protected override void ConvertChildren()
        {
            base.ConvertChildren();

            if(null != Control.Content)
            {
                Context.Convert(Control.Content, Writer);
            }
        }
    }
}
