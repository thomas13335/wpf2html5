using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Wpf2Html5.Converter.Framework
{
    class PasswordBoxConverter : FrameworkElementConverter<PasswordBox>
    {
        public override string HtmlTag { get { return "INPUT"; } }

        protected override void WriteAttributes()
        {
            AddCssClass("textbox");
            Writer.WriteAttributeString("type", "password");

            /*if (MapProperty(PasswordBox.Password))
            {
                EnableKeyBindings();
            }*/

            base.WriteAttributes();
        }

        protected override void StretchHorizontally()
        {
            Style.Add("width", "98%");
        }
    }
}
