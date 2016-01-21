using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Wpf2Html5.Converter.Framework
{
    class ItemsControlConverter : FrameworkElementConverter<ItemsControl>
    {
        public override string HtmlTag
        {
            get { return "DIV"; }
        }

        protected override void MapCommonProperties()
        {
            base.MapCommonProperties();

            MapProperty(ItemsControl.ItemsSourceProperty);

            if (Control.ItemTemplate != null)
            {
                ProcessDataTemplate(Control.ItemTemplate);
                Writer.WriteAttributeString("data-template-context", ID);
            }
        }

        protected override void ConvertChildren()
        {
            base.ConvertChildren();

            Writer.WriteString("\u00A0");
        }
    }
}
