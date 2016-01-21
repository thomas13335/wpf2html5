using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Xml;
using Wpf2Html5.Converter.Interface;
using Wpf2Html5.Style;

namespace Wpf2Html5.Converter.Framework
{
    /// <summary>
    /// Baseclass for WPF to HTML control converters.
    /// </summary>
    /// <typeparam name="T">The control type to implement.</typeparam>
    class ControlConverterBase<T> : IControlConverter where T : class
    {
        #region Private

        private string _id;

        #endregion

        #region Protected Properties

        protected XmlWriter Writer { get; private set; }

        protected StyleBuilder Style { get; private set; }

        protected ConverterArguments Arguments { get; private set; }

        protected List<string> CssClasses { get; private set; }

        protected string DataTemplateContext { get; set; }

        #endregion

        #region Public Properties

        public IConverterContext Context { get; set; }

        public virtual string ID { get { return AllocateId(); } }

        public virtual string HtmlTag { get { return null; } }

        public virtual Type ControlType { get { return Control.GetType(); } }

        public virtual DisplayKind Display { get { return DisplayKind.inline; } }

        public virtual bool IsStretchable { get { return true; } }

        public T Control { get; set; }

        public bool IsSubClass { get; set; }

        public virtual bool IsTypeRegistrable { get { return true; } }

        #endregion

        #region Diagnostics

        protected virtual void Trace(string format, params object[] args)
        {
            Log.Trace(format, args);
        }

        protected virtual void Warning(string format, params object[] args)
        {
            Log.Warning(format, args);
        }

        #endregion

        #region Public Methods

        public void SetControl(object obj)
        {
            if (null == (Control = obj as T))
            {
                throw new Exception("unable to set control on control converter.");
            }
        }

        public virtual void Convert(XmlWriter writer, ConverterArguments args)
        {
            try
            {
                Arguments = args;
                Writer = writer;

                ConvertOverride();
            }
            finally
            {
                Writer = null;
                Arguments = null;
            }
        }

        /// <summary>
        /// Maps a property binding, if present.
        /// </summary>
        /// <param name="dprop">The property to binding.</param>
        /// <returns>True if a binding exists, false otherwise.</returns>
        public bool MapProperty(DependencyProperty dprop)
        {
            // expect a dependency object control ...
            var dobj = Control as DependencyObject;
            if (null != dobj)
            {
                // look for binding for that property ..
                var binding = BindingOperations.GetBinding(dobj, dprop);
                var id = AllocateId();
                if (null != binding)
                {
                    // binding existing, copy it.
                    Context.AddBinding(this, dprop.Name, binding);
                    return true;
                }
                else
                {
                    // binding does not exist
                    return false;
                }
            }
            else
            {
                // not a suitable control
                return false;
            }
        }

        #endregion

        #region Overrideables

        protected virtual void ConvertOverride()
        {
            WriteStart();

            try
            {
                // allocate a style builder for this conversion ...
                Style = new StyleBuilder();
                CssClasses = new List<string>();

                if (null != Arguments && null != Arguments.CssClasses)
                {
                    CssClasses.AddRange(Arguments.CssClasses);
                }


                // translate and emit attributes
                WriteAttributes();

                // summarize style and classes
                if (CssClasses.Count > 0)
                {
                    Writer.WriteAttributeString("class", CssClasses.ToSeparatorList(" "));
                }

                if (Style.Count > 0)
                {
                    Writer.WriteAttributeString("style", Style.ToString());
                }

                // convert content
                ConvertChildren();
            }
            finally
            {
                CssClasses = null;
                Style = null;
            }

            WriteEnd();
        }

        protected virtual void ConvertChildren()
        {

        }

        protected virtual void WriteAttributes()
        {
            WritePresentationTypeName();

            var nowrapper = Control.GetType().GetCustomAttributes(false).OfType<GeneratorNoWrapperAttribute>().Any();

            if (IsSubClass && !nowrapper)
            {
                // RDLN7CYSPV: set wrapper creation flag.
                Writer.WriteAttributeString("wrapper", "true");
            }
        }

        protected virtual void WriteStart()
        {
            if (null == HtmlTag)
            {
                throw new Exception("no HTML tag defined for [" + this + "].");
            }

            Writer.WriteStartElement(HtmlTag);
        }

        protected virtual void WriteEnd()
        {
            Writer.WriteEndElement();
        }

        protected virtual void WritePresentationTypeName()
        {
            var ctype = Control.GetType().Name;
            Writer.WriteAttributeString("data-ctype", ctype);
        }

        #endregion

        #region Protected Methods

        protected void AddCssClass(string name)
        {
            if (!CssClasses.Contains(name)) CssClasses.Add(name);
        }

        protected string SetID(string id)
        {
            if(null == _id)
            {
                _id = id;
            }
            else if(_id != id)
            {
                throw new Exception("ID is already set.");
            }

            return _id;
        }

        protected virtual string AllocateId()
        {
            if (null == _id)
            {
                _id = Context.AllocateIdentifier(this);
            }

            return _id;
        }

        protected string AllocateId(XmlWriter writer)
        {
            var id = AllocateId();
            writer.WriteAttributeString("id", id);
            return id;
        }

        #endregion

        #region Code Generation

        public virtual string GetSupportMethodName(SupportMethodID methodid)
        {
            return null;
        }

        #endregion

    }
}
