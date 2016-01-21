using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wpf2Html5.StockObjects;

namespace Wpf2Html5.Converter.Framework
{
    class UIElementConverter<T> : DependencyObjectConverter<T> where T : UIElement
    {
        protected virtual void MapCommonProperties()
        {
            if(MapProperty(UIElement.VisibilityProperty))
            {

            }
        }
    }
}
