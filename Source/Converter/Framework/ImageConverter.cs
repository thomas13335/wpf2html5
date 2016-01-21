using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Wpf2Html5.Style;

namespace Wpf2Html5.Converter.Framework
{
    class ImageConverter : FrameworkElementConverter<Image>
    {
        public override string HtmlTag
        {
            get { return "IMG"; }
        }

        public override DisplayKind Display
        {
            get { return DisplayKind.inline_block; }
        }

        public override bool IsStretchable { get { return false; } }


        protected override void MapCommonProperties()
        {
            base.MapCommonProperties();

            if (!MapProperty(Image.SourceProperty))
            {
                if (null != Control.Source)
                {
                    Writer.WriteAttributeString("src", Control.Source.ToString());
                }
            }

            if(!MapProperty(Image.StretchProperty))
            {
                if (!Style.AllKeys.Contains("width") && Control.Stretch != System.Windows.Media.Stretch.None)
                {
                    Style.Add("width", "100%");
                }
            }
        }

        protected override void ConvertChildren()
        {
            base.ConvertChildren();
        }
    }
}
