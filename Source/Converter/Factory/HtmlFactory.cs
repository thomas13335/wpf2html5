using System;
using System.Collections.Generic;
using System.Windows;
using System.Xml;
using Wpf2Html5.Converter.Framework;
using Wpf2Html5.Converter.Interface;
using Wpf2Html5.Exceptions;

namespace Wpf2Html5.Factory
{
    /// <summary>Constructs control converters from WPF controls.</summary>
    public static class HtmlFactory
    {
        #region Private

        private static Dictionary<Type, Type> _typemap = new Dictionary<Type, Type>();

        static HtmlFactory()
        {
            /*_typemap[typeof(StackPanel)] = typeof(StackPanelConverter);
            _typemap[typeof(ItemsControl)] = typeof(ItemsControlConverter);
            _typemap[typeof(TextBlock)] = typeof(TextBlockConverter);
            _typemap[typeof(TextBox)] = typeof(TextBoxConverter);
            _typemap[typeof(Border)] = typeof(BorderConverter);*/
            _typemap[typeof(FrameworkElement)] = typeof(GenericFrameworkElementConverter);
        }

        #endregion

        /// <summary>Creates a <see cref="ControlConverter"/> for a control.</summary>
        /// <param name="obj">The control to convert.</param>
        /// <param name="context">The context in which the conversion takes place.</param>
        /// <param name="writer">XML writer to receive the converted HTML.</param>
        /// <param name="args">Arguments for the conversion.</param>
        /// <returns></returns>
        public static IControlConverter Convert(object obj, IConverterContext context, XmlWriter writer, ConverterArguments args)
        {
            Type originaltype, type, convertertype = null;

            // ascend the derivation path until ...
            type = originaltype = obj.GetType();
            while (null != type)
            {
                // a previously used converter matches
                if (_typemap.TryGetValue(type, out convertertype))
                {
                    break;
                }

                // or a converter existings in the Factory namespace.
                if (null != (convertertype = LookupConverter(type)))
                {
                    break;
                }

                type = type.BaseType;
            }

            if (null == convertertype)
            {
                throw new Exception("unable to convert [" + obj.GetType().FullName + "].");
            }

            // using a baseclass converter?
            var baseclassconverter = type != originaltype;

            /*if(originaltype.GetCustomAttributes(false).OfType<GeneratorIgnoreAttribute>().Any())
            {
                baseclassconverter = false;
            }*/

            // TraceTarget.Trace("using [{0}] to convert [{1}] ...", convertertype.FullName, originaltype.FullName);

            // construct the converter object
            var constructor = convertertype.GetConstructor(new Type[0]);
            if (null == constructor)
            {
                throw new ConverterException(ErrorCode.InvalidConverterType, 
                    "type [" + convertertype.FullName + "] constructor unavailable.");
            }

            var converter = constructor.Invoke(new object[0]) as IControlConverter;
            if (null == converter)
            {
                throw new ConverterException(ErrorCode.InvalidConverterType, 
                    "type [" + convertertype.FullName + "] does not support converter interface.");
            }

            converter.Context = context;
            converter.IsSubClass = baseclassconverter;
            converter.SetControl(obj);
            converter.Convert(writer, args);

            if (converter.IsTypeRegistrable)
            {
                // add reference ...
                context.TriggerItemReference(originaltype.FullName);
            }

            return converter;
        }

        #region Private Methods

        private static Type LookupConverter(Type type)
        {
            var vtype = typeof(ControlConverterBase<>);
            var ns = vtype.Namespace;
            var fullname = ns + "." + type.Name + "Converter";

            return vtype.Assembly.GetType(fullname);
        }

        #endregion
    }
}
