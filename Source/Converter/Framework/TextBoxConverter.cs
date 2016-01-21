using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Markup.Primitives;
using System.Xml;
using Wpf2Html5.Style;

namespace Wpf2Html5.Converter.Framework
{
    class TextBoxConverter : FrameworkElementConverter<TextBox>
    {
        public override string HtmlTag { get { return "INPUT"; } }

        protected override void WriteAttributes()
        {
            AddCssClass("textbox");
            Writer.WriteAttributeString("type", "text");

            if (MapProperty(TextBox.TextProperty))
            {
                EnableKeyBindings();
            }

            base.WriteAttributes();
        }

        protected override void StretchHorizontally()
        {
            Style.Add("width", "98%");
        }
    }
}
