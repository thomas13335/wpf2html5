using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Wpf2Html5.Converter.Framework
{
    class GenericFrameworkElementConverter : DependencyObjectConverter<FrameworkElement>
    {
        public override string HtmlTag
        {
            get
            {
                return "DIV";
            }
        }

        protected override void ConvertChildren()
        {
            base.ConvertChildren();

            Writer.WriteString("[" + Control.GetType().Name + "]");
        }
    }
}
