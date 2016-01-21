using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Wpf2Html5.Converter.Framework
{
    /// <summary>
    /// Converts a UserControl (6HG6FOT7NZ)
    /// </summary>
    class UserControlConverter : ControlConverter<UserControl>
    {
        public override string HtmlTag
        {
            get { return "DIV"; }
        }

        protected override void ConvertChildren()
        {
            base.ConvertChildren();

            var type = Control.GetType();
            if (!type.GetCustomAttributes(false).OfType<GeneratorIgnoreAttribute>().Any())
            {
                int childcount = 0;

                if (null != Control.Content)
                {
                    Context.Convert(Control.Content, Writer);
                    childcount++;
                }

                if (0 == childcount)
                {
                    Writer.WriteString("\u00a0");
                }
            }
            else
            {
                Writer.WriteString("\u00a0");
            }
        }
    }
}
