using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Wpf2Html5.StockObjects
{
    [GeneratorIgnore]
    public class DateTimeToPrintableConverter : IValueConverter, IJScriptConvertibleConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var t = (DateTime)value;

            return t.ToString(CultureInfo.CurrentCulture);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public string GenerateJScript()
        {
            return "new DateTimeToPrintableConverter()";
        }
    }
}
