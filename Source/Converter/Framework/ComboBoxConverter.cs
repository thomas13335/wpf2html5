using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Wpf2Html5.Style;

namespace Wpf2Html5.Converter.Framework
{
    class ComboBoxConverter : FrameworkElementConverter<ComboBox>
    {
        public override string HtmlTag
        {
            get { return "SELECT"; }
        }

        protected override void MapCommonProperties()
        {
            base.MapCommonProperties();

            MapProperty(ItemsControl.ItemsSourceProperty);

            if (MapProperty(ComboBox.SelectedItemProperty))
            {
                Writer.WriteAttributeString("onchange", "ComboBox_selectionchanged(this);");
            }
        }

        protected override void ConvertChildren()
        {
            base.ConvertChildren();

            Writer.WriteString("\u00A0");
        }
    }
}
