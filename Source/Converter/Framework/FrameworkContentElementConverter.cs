using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace Wpf2Html5.Converter.Framework
{
    class FrameworkContentElementConverter<T> : DependencyObjectConverter<T> where T : FrameworkContentElement
    {
    }
}
