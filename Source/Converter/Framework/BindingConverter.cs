using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Wpf2Html5.Builder;

namespace Wpf2Html5.Converter.Framework
{
    /// <summary>
    /// Converts bindings into corresponding JS initialization calls (G4M6XZEASP).
    /// </summary>
    class BindingConverter
    {
        #region Private

        private JScriptWriter _writer;

        #endregion

        #region Construction

        public BindingConverter(JScriptWriter writer)
        {
            _writer = writer;
        }

        #endregion

        public void AddBinding(string target, string prop, Binding binding)
        {
            var parameters = new List<string>();
            parameters.Add(target);
            parameters.Add(JScriptWriter.QuoteString(prop));
            parameters.Add("datacontext");
            parameters.Add(JScriptWriter.QuoteString(ConvertPropertyPath(binding.Path)));

            if (null != binding.Converter)
            {
                // convert the converter ...
                parameters.Add(ConvertConverter(binding.Converter));
            }
            else
            {
                parameters.Add("null");
            }

            _writer.WriteFunctionCall("CreateBindingObject", parameters);
        }

        public void AddKeyBinding(string target, KeyBinding kb)
        {
            var commandbinding = BindingOperations.GetBinding(kb, InputBinding.CommandProperty);
            var parameterbinding = BindingOperations.GetBinding(kb, InputBinding.CommandParameterProperty);

            var source = "datacontext";

            string cb, sb;
            if (!ConvertBinding(commandbinding, source, out cb))
            {
                Log.Warning("binding has not Command set.");
                return;
            }

            ConvertBinding(parameterbinding, source, out sb);

            var fkey = KeyInterop.VirtualKeyFromKey(kb.Key);
            var name = "__key_" + (int)fkey;

            string js = "new KeyBinding(" + target + ", '" + name + "', " + cb + ", " + sb + ");";
            _writer.WriteLine(js);
        }

        public void AddMouseBinding(string target, MouseBinding mb)
        {
            var commandbinding = BindingOperations.GetBinding(mb, InputBinding.CommandProperty);
            var parameterbinding = BindingOperations.GetBinding(mb, InputBinding.CommandParameterProperty);

            var source = "datacontext";

            string cb, sb;
            if (!ConvertBinding(commandbinding, source, out cb))
            {
                Log.Warning("binding has not Command set.");
                return;
            }

            ConvertBinding(parameterbinding, source, out sb);

            string js = "new MouseBinding(" + target + ", " + ConvertMouseAction(mb.MouseAction) + ", " + cb + ", " + sb + ");";
            _writer.WriteLine(js);
        }

        #region Private Methods

        private string ConvertConverter(IValueConverter converter)
        {
            var jsc = converter as IJScriptConvertibleConverter;
            if (null == jsc)
            {
                throw new Exception("converter " + converter + " does not support JScript conversion.");
            }

            return jsc.GenerateJScript();
        }

        /// <summary>
        /// Converts a binding into a jscript expression.
        /// </summary>
        /// <param name="b">The binding to convert.</param>
        /// <param name="source">The source object for the binding.</param>
        /// <param name="code">The resulting JS expression.</param>
        /// <returns></returns>
        private bool ConvertBinding(Binding b, string source, out string code)
        {
            if(null == b)
            {
                code = "null";
                return false;
            }
            else
            {
                var path = ConvertPropertyPath(b.Path);
                code = "new Binding('" + path + "', " + source + ")";
                return true;
            }
        }

        private string ConvertPropertyPath(PropertyPath ppath)
        {
            return null == ppath ? string.Empty : ppath.Path;
        }

        private string ConvertMouseAction(MouseAction action)
        {
            return "'__mouse_" + action + "'";
        }

        #endregion
    }
}
