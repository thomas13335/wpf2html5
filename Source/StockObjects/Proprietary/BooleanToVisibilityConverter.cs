using System;
using System.Windows;
using System.Windows.Data;

namespace Wpf2Html5.StockObjects
{
    [GeneratorIgnore]
    public class BooleanToVisibilityConverter : IValueConverter, IJScriptConvertibleConverter
    {
        public Visibility Invisibility { get; set; }

        public bool Invert { get; set; }

        public BooleanToVisibilityConverter()
        {
            Invisibility = Visibility.Collapsed;
        }

        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return ((bool)value ^ Invert) ? Visibility.Visible : Invisibility;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IJScriptConvertibleConverter

        public string GenerateJScript()
        {
            return "new BooleanToVisibilityConverter(\"" + Invisibility.ToString().ToLower() + "\", " + Invert.ToString().ToLower() + ")";
        }

        #endregion
    }
}
