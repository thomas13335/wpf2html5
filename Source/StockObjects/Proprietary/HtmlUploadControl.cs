using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Wpf2Html5.StockObjects
{
    [GeneratorIgnore]
    [GeneratorNoWrapperAttribute]
    public class HtmlUploadControl : Panel
    {


        public object FileObject
        {
            get { return (object)GetValue(FileObjectProperty); }
            set { SetValue(FileObjectProperty, value); }
        }

        public static readonly DependencyProperty FileObjectProperty =
            DependencyProperty.Register("FileObject", typeof(object), typeof(HtmlUploadControl));

        
    }
}
