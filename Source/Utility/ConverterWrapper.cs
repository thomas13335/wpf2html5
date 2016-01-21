using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wpf2Html5.Converter.Configuration;
using Wpf2Html5.Converter.Model;

namespace Wpf2Html5.Utility
{
    /// <summary>
    /// Host the ConverterModel in a suitable (remoteable) context.
    /// </summary>
    public class ConverterWrapper : MarshalByRefObject
    {
        public ConverterWrapper()
        {
        }

        public TraceOptions Flags { get; set; }

        public int WarningCount { get; private set; }

        public int ErrorCount { get; private set; }

        public void Convert(string projectfile, string outputpath)
        {
            var app = new Application();
            var project = ProjectSettings.Load(projectfile);

            var jsf = Path.Combine(outputpath, "converter.settings.js");
            project.Save(jsf);

            var model = new ConverterModel();
            model.PageTitle = project.PageTitle;
            model.IsOptimized = false;
            model.InlineAllScript = true;
            model.OutputDirectory = outputpath;
            // model.IncludeResources = new string[] { "base.css", "application.css", "*.png", "octicons.*" };
            model.TraceOutputFile = "converter.output.html";

            // trace flags
            if (null != Flags)
            {
                Log.ShowVerbose = Flags.Verbose;
                Log.ShowConverterModel = Flags.ShowFiles;
                Log.ShowResources = Flags.ShowResources;

                if (Flags.ShowClasses)
                {
                    Log.ShowClassDependencies = Flags.ShowClasses;
                }
            }

            // setting the project property triggers the conversion process
            model.Project = project;

            WarningCount = model.Converter.WarningCount;
            ErrorCount = model.Converter.ErrorCount;
        }
    }
}
