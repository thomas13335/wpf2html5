using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Wpf2Html5.Converter.Framework
{
    class PanelConverterBase<T> : FrameworkElementConverter<T> where T : Panel
    {
        protected override void MapCommonProperties()
        {
            base.MapCommonProperties();

            if(!MapProperty(Panel.BackgroundProperty))
            {
                if (null != Control.Background)
                {
                    Style.AddBrush(Control.Background, "background");
                }
            }
        }
    }

    class PanelConverter : PanelConverterBase<Panel>
    {
        public override string HtmlTag { get { return "div"; } }

        protected override void MapCommonProperties()
        {
            base.MapCommonProperties();

            var controltype = Control.GetType();
            if(controltype != typeof(Panel))
            {
                // derivate, check properties
                foreach(var field in controltype.GetFields(BindingFlags.Static | BindingFlags.Public)
                    .Where(f => f.DeclaringType == controltype)
                    .Where(f => f.FieldType == typeof(DependencyProperty)))
                {
                    var dp = field.GetValue(null) as DependencyProperty;
                    MapProperty(dp);
                }
            }
        }

        protected override void ConvertChildren()
        {
            base.ConvertChildren();

            if(Control.Children.Count == 0)
            {
                Writer.WriteString("\u00a0");
            }
        }
    }
}
