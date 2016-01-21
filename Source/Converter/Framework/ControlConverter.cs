using System.Windows.Controls;

namespace Wpf2Html5.Converter.Framework
{
    class ControlConverter<T> : FrameworkElementConverter<T> where T : Control
    {
        protected override void MapCommonProperties()
        {
            base.MapCommonProperties();

            var paddingproperty = System.Windows.Controls.Control.PaddingProperty;
            if (!MapProperty(paddingproperty))
            {
                if (IsPropertyLocal(paddingproperty))
                {
                    Style.AddThickness(Control.Padding, "padding");
                }
            }

        }
    }

    class ControlConverter : ControlConverter<Control>
    {
        public override string HtmlTag { get { return "div"; } }

        protected override void MapCommonProperties()
        {
            base.MapCommonProperties();
        }

        protected override void ConvertChildren()
        {
            base.ConvertChildren();

            Writer.WriteString("\u00a0");
        }
    }

}
