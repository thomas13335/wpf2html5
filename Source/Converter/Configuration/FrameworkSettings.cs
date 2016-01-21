using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf2Html5.Converter.Configuration
{
    public class FrameworkSettings : ProjectSettings
    {
        public FrameworkSettings(string projectdirectory) : base(projectdirectory)
        {
            Name = "WPF2HTML5 Framework 1.0";
            //SourceDirectory = Path.Combine(projectdirectory, @"Source\Tools\Wpf2Html5");
            SourceDirectory = projectdirectory;
            Projects = new string[] { "Wpf2Html5.StockObjects.csproj" };
            AdditionalAssemblies = new string[] { "Wpf2Html5.Runtime" };

            FrameworkScripts = new string[] { 
                "trace.js",
                "ready.js",
                "json2.js",
                "List.js",
                "DataTypes.js",
                "DependencyObject.js",
                "Controls.js",
                "Converters.js",
                "ItemsControl.js",
                "BindingManager.js",
                "ControlFactory.js",
                "Binding.js",
                "BindingObject.js",
                "InputBinding.js",
                "ObservableCollection.js",
                "WebViewModelBase.js",
                "DragDrop.js",
                "TemplateFactory.js",
                "LocalStorageAccessor.js",
                "PathOperations.js",
                "DataContextInitialization.js",
                "HtmlUploadControl.js",
                "PathControl.js",
                "Dictionary.js",
                "ResizeManager.js",
                "PleaseWaitControl.js",
                "stock.js",
                "JsonWebRequest.js",
                "StringValidation.js"
            };
        }
    }
}
