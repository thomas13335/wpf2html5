using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Wpf2Html5.Style;

namespace Wpf2Html5.Converter.Framework
{
    class ButtonConverter : ControlConverter<Button>
    {
        public override string HtmlTag
        {
            get { return "BUTTON"; }
        }

        protected override void MapCommonProperties()
        {
            base.MapCommonProperties();

            if (MapProperty(Button.CommandProperty))
            {
                Writer.WriteAttributeString("onclick", "Button_Click(this);");
            }

            if(!MapProperty(Button.ContentProperty))
            {

            }

            if(!MapProperty(Button.IsDefaultProperty))
            {
                if(IsPropertyLocal(Button.IsDefaultProperty))
                {
                    // CKEKBFTP4K: IsDefault property on button
                    Writer.WriteAttributeString("data-isdefault", Control.IsDefault.ToString().ToLower());
                }
            }

            MapProperty(Button.CommandParameterProperty);
        }

        protected override void ConvertChildren()
        {
            base.ConvertChildren();

            if (Control.Content is string)
            {
                Writer.WriteString(Control.Content.ToString());
            }
            else if (null != Control.Content)
            {
                Context.Convert(Control.Content, Writer);
            }
        }
    }
}
