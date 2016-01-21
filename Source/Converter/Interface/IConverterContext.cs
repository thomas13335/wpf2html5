using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Xml;
using Wpf2Html5.Builder;

namespace Wpf2Html5.Converter.Interface
{
    public interface IConverterContext : IConverterStack
    {
        string AllocateIdentifier(IControlConverter converter);

        IControlConverter Convert(object obj, XmlWriter writer, IConverterContext context = null, ConverterArguments args = null);

        JScriptBuilder CodeBuilder { get; }

        JScriptBuilder Declarations { get; }

        void AddBinding(IControlConverter target, string prop, Binding b);

        void AddKeyBinding(IControlConverter target, KeyBinding kb);

        void AddMouseBinding(IControlConverter target, MouseBinding mb);

        void AddSizeBinding(IControlConverter container, string suffix);

        void RegisterDataTemplate(DataTemplate template, DataTemplateInfo info);

        void GenerateDataTemplate(DataTemplate template, XmlWriter writer, IConverterContext context);

        IControlConverter GetControlByID(string id);

        void TriggerItemReference(string typename);
    }
}
