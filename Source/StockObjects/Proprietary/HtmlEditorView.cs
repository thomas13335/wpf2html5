using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Wpf2Html5.StockObjects
{
    /// <summary>
    /// An HTML editor control.
    /// </summary>
    /// <remarks>
    /// <para>Do not create wrapper too early, as this is a dynamic control (RDLN7CYSPV).</para>
    /// </remarks>
    [GeneratorIgnore]
    [GeneratorNoWrapper]
    public class HtmlEditorView : UserControl
    {
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(HtmlEditorView));

        
    }
}
