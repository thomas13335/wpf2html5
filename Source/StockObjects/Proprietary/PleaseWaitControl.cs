using System.Windows;
using System.Windows.Controls;

namespace Wpf2Html5.StockObjects
{
    [GeneratorIgnore]
    // [GeneratorNoWrapper]
    public class PleaseWaitControl : UserControl
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(PleaseWaitControl));

        
    }
}
