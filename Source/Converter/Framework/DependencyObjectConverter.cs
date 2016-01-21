using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wpf2Html5.StockObjects;

namespace Wpf2Html5.Converter.Framework
{
    class DependencyObjectConverter<T> : ControlConverterBase<T> where T : DependencyObject
    {
        protected override void WriteAttributes()
        {
            base.WriteAttributes();

            var cls = CssClass.GetCssClasses(Control);
            if(null != cls)
            {
                AddCssClass(cls);
            }
        }

        protected virtual bool IsPropertyLocal(DependencyProperty dprop)
        {
            var source = DependencyPropertyHelper.GetValueSource(Control, dprop);

            switch(source.BaseValueSource)
            {
                case BaseValueSource.Inherited:
                case BaseValueSource.Default:
                    return false;

                default:
                    return true;
            }
        }
    }
}
