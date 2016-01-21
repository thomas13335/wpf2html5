using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Wpf2Html5.StockObjects
{
    [GeneratorIgnore]
    public class BooleanInverterConverter : IValueConverter, IJScriptConvertibleConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(bool)value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return !(bool)value;
        }

        public string GenerateJScript()
        {
            return "new BooleanInverterConverter()";
        }
    }
}
