using System.Collections.Generic;

namespace Wpf2Html5.Converter.Interface
{
    public class ConverterArguments
    {
        public IEnumerable<string> CssClasses { get; set; }

        public ConverterArguments() { }

        public ConverterArguments(params string[] cssclasses) { CssClasses = cssclasses; }
    }
}
