using System.Windows;
using System.Windows.Controls;

namespace Wpf2Html5.StockObjects
{
    /// <summary>
    /// A border that changes class when the mouse is over it.
    /// </summary>
    [GeneratorIgnore]
    public class BorderWithMouse : Border
    {
        /// <summary>
        /// The CSS class to add when the mouse is over.
        /// </summary>
        public string CssClass
        {
            get { return (string)GetValue(CssClassProperty); }
            set { SetValue(CssClassProperty, value); }
        }

        public static readonly DependencyProperty CssClassProperty =
            DependencyProperty.Register("CssClass", typeof(string), typeof(BorderWithMouse));
        
    }
}
