using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Wpf2Html5.Style;

namespace Wpf2Html5.Converter.Framework
{
    class TextBlockConverter : FrameworkElementConverter<TextBlock>
    {
        public override string HtmlTag { get { return "SPAN"; } }

        protected override void MapCommonProperties()
        {
            base.MapCommonProperties();

            if (!MapProperty(TextBlock.FontWeightProperty))
            {
                if (IsPropertyLocal(TextBlock.FontWeightProperty))
                {
                    Style.Add("font-weight", Control.FontWeight.ToString().ToLower());
                }
            }

            if (!MapProperty(TextBlock.FontSizeProperty))
            {
                if (IsPropertyLocal(TextBlock.FontSizeProperty))
                {
                    Style.Add("font-size", Control.FontSize + "px");
                }
            }

            if (!MapProperty(TextBlock.FontFamilyProperty))
            {
                if (IsPropertyLocal(TextBlock.FontFamilyProperty))
                {
                    Style.Add("font-family", Control.FontFamily.ToString());
                }
            }

            if (!MapProperty(TextBlock.PaddingProperty))
            {
                if (IsPropertyLocal(TextBlock.PaddingProperty))
                {
                    Style.AddThicknessOptional(Control.Padding, "padding");
                }
            }

            if (!MapProperty(TextBlock.ForegroundProperty))
            {
                if (IsPropertyLocal(TextBlock.ForegroundProperty))
                {
                    Style.AddBrush(Control.Foreground, "color");
                }
            }

            if (!MapProperty(TextBlock.TextProperty))
            {
                // TODO: static text? --> 7A27DOZU5F: done in the template generator, not so good.
            }
        }

        protected override void ConvertChildren()
        {
            base.ConvertChildren();

            Context.Push(DisplayKind.inline);

            if (null == Control.Inlines || !Control.Inlines.Any())
            {
                Writer.WriteString(Control.Text);
            }
            else
            {
                foreach (var child in Control.Inlines)
                {
                    Context.Convert(child, Writer);
                }
            }

            Context.Pop();
        }
    }
}
