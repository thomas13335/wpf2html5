using System.Windows.Controls;
using Wpf2Html5.Converter.Interface;

namespace Wpf2Html5.Converter.Framework
{
    class StackPanelConverter : PanelConverterBase<StackPanel>
    {
        public override string HtmlTag
        {
            get { return "DIV"; }
        }

        protected override void MapCommonProperties()
        {
            AddCssClass("stackpanel-" + Control.Orientation.ToString().ToLower());

            base.MapCommonProperties();
        }

        protected override void ConvertChildren()
        {
            base.ConvertChildren();

            // push the current orientation onto the stack ...
            Context.Push(Control.Orientation);

            int childcount = 0;
            foreach (var child in Control.Children)
            {
                Context.Convert(child, Writer, null, new ConverterArguments("stackitem"));

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
