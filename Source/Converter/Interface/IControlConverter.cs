using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Wpf2Html5.Converter.Interface
{
    /// <summary>
    /// Interface exposed by control converters.
    /// </summary>
    public interface IControlConverter
    {
        /// <summary>
        /// The associated conversion context, set by the factory.
        /// </summary>
        IConverterContext Context { get; set; }

        /// <summary>
        /// The type of the underlying control.
        /// </summary>
        Type ControlType { get; }

        /// <summary>
        /// Per conversion session identifier.
        /// </summary>
        string ID { get; }

        /// <summary>
        /// Associates a control with the converter.
        /// </summary>
        /// <param name="obj">The control object.</param>
        /// <remarks>This must be called before the Convert method.</remarks>
        void SetControl(object obj);

        /// <summary>
        /// Converts the control to HTML.
        /// </summary>
        /// <param name="writer">The XML writer receiving the HTML.</param>
        /// <param name="args">Optional converter arguments.</param>
        void Convert(XmlWriter writer, ConverterArguments args = null);

        /// <summary>
        /// True if the converter is for a baseclass of the control.
        /// </summary>
        bool IsSubClass { get; set; }

        /// <summary>
        /// True if the control type shall be registered in the type system.
        /// </summary>
        bool IsTypeRegistrable { get; }

        string GetSupportMethodName(SupportMethodID methodid);
    }
}
