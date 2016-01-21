using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf2Html5.Converter.Interface;

namespace Wpf2Html5.Converter.Framework
{
    class StringConverter : ControlConverterBase<string>
    {
        public override void Convert(System.Xml.XmlWriter writer, ConverterArguments args)
        {
            writer.WriteString(Control);
        }
    }
}
