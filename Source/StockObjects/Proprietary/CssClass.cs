using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Wpf2Html5.StockObjects
{
    [GeneratorIgnore]
    public class CssClass : DependencyObject
    {
        public static readonly DependencyProperty CssClassesProperty =
        DependencyProperty.RegisterAttached(
            "CssClasses",
            typeof(string),
            typeof(CssClass),
            new PropertyMetadata(null)
        );

        public static void SetCssClasses(DependencyObject element, string value)
        {
            element.SetValue(CssClassesProperty, value);
        }

        public static string GetCssClasses(DependencyObject element)
        {
            return (string)element.GetValue(CssClassesProperty);
        }

    }
}
