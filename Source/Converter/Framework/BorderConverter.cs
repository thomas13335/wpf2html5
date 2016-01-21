using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Xml;
using Wpf2Html5.Style;

namespace Wpf2Html5.Converter.Framework
{
    class BorderConverter : FrameworkElementConverter<Border>
    {
        public override string HtmlTag
        {
            get { return "DIV"; }
        }

        public override DisplayKind Display
        {
            get { return DisplayKind.block; }
        }

        protected override void WriteAttributes()
        {
            base.WriteAttributes();
        }

        protected override void MapCommonProperties()
        {
            base.MapCommonProperties();

            if (!MapProperty(Border.BorderThicknessProperty))
            {
                if (IsPropertyLocal(Border.BorderThicknessProperty))
                {
                    Style.AddThickness(Control.BorderThickness, "border", "width");
                    Style.Add("border-style", "solid");
                }
            }

            if (!MapProperty(Border.PaddingProperty))
            {
                if (IsPropertyLocal(Border.PaddingProperty))
                {
                    Style.AddThicknessOptional(Control.Padding, "padding");
                }
            }

            if (!MapProperty(Border.CornerRadiusProperty))
            {
                if (IsPropertyLocal(Border.CornerRadiusProperty))
                {
                    Style.AddCornerRadius(Control.CornerRadius, "border-radius");
                }
            }

            if (!MapProperty(Border.BorderBrushProperty))
            {
                if (IsPropertyLocal(Border.BorderBrushProperty))
                {
                    Style.AddBrush(Control.BorderBrush, "border-color");
                }
            }

            if (!MapProperty(Border.BackgroundProperty))
            {
                if (IsPropertyLocal(Border.BackgroundProperty))
                {
                    Style.AddBrush(Control.Background, "background");
                }
            }
        }

        protected override void ConvertChildren()
        {
            base.ConvertChildren();

            if (null != Control.Child)
            {
                Context.Convert(Control.Child, Writer);
            }
            else
            {
                Writer.WriteString("\u00A0");
            }

        }
    }
}
