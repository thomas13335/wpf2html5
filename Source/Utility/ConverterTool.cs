using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Wpf2Html5.Utility;

namespace Wpf2Html5
{
    /// <summary>
    /// Performs a WPF to HTML conversion based on a settings file.
    /// </summary>
    public class ConverterTool
    {
        #region Properties

        public string ProjectFile { get; set; }

        public string WorkingDirectory { get; set; }

        public string OutputPath { get; set; }

        public string OutputFile { get; set; }

        public Exception Error { get; private set; }

        public TraceOptions Flags { get; private set; }

        #endregion

        #region Construction

        public ConverterTool()
        {
            WorkingDirectory = Directory.GetCurrentDirectory();
            Flags = new TraceOptions();
        }

        #endregion

        #region Diagnostics

        public void Trace(string format, params object[] args)
        {
            if (Debugger.IsAttached)
            {
                Debug.WriteLine(format, args);
            }

            Console.WriteLine(format, args);
        }

        #endregion

        #region Public Methods

        public int Execute(string args)
        {
            if(!ParseArgs(args))
            {
                Trace("WPF to HTML5 converter, v{0}", GetType().Assembly.GetName().Version.ToString(4));
                Trace("syntax: [ -o <output> ] <settings-file>");
                return 1;
            }

            if(!File.Exists(ProjectFile))
            {
                Trace("error: project file not found.");
                return 1;
            }

            if(null == OutputPath)
            {
                OutputPath = Directory.GetCurrentDirectory();
            }

            Trace("processing project file '{0}' ...", ProjectFile);
            
            var thread = new Thread(Run);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start(null);
            thread.Join();

            return null == Error ? 0 : 2;
        }

        #endregion

        #region Private Methods

        private void Run(object obj)
        {
            var appdom = AppDomain.CreateDomain("Wpf2Html5Build");

            try
            {
                try
                {
                    var type = typeof(ConverterWrapper);
                    var h = appdom.CreateInstance(type.Assembly.FullName, type.FullName);
                    var wrapper = h.Unwrap() as ConverterWrapper;
                    wrapper.Flags = Flags;

                    wrapper.Convert(ProjectFile, OutputPath);

                    Trace("conversion completed with {0} error(s) and {1} warning(s).", wrapper.ErrorCount, wrapper.WarningCount);
                }
                catch (Exception ex)
                {
                    Log.Error("error: {0}", ex.Message);
                    Error = ex;
                }
            }
            finally
            {
                AppDomain.Unload(appdom);
            }
        }

        private bool ParseArgs(string args0)
        {
            var args = args0.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int s = 1;

            for(int j = 0; j < args.Length; ++j)
            {
                var arg = args[j];

                if (1 == s)
                {
                    if (arg.StartsWith("-"))
                    {
                        switch (arg)
                        {
                            case "-o":
                            case "--output":
                                s = 2;
                                break;

                            case "-v": Flags.Verbose = true; break;
                            case "-f": Flags.ShowFiles = true; break;
                            case "-r": Flags.ShowResources = true; break;
                            case "-c": Flags.ShowClasses = true; break;
                        }
                    }
                    else
                    {
                        ProjectFile = MakeRootedPath(arg);
                    }
                }
                else if(2 == s)
                {
                    OutputPath = MakeRootedPath(arg);
                    s = 1;
                }
            }

            return null != ProjectFile;
        }

        private string MakeRootedPath(string arg)
        {
            if (!Path.IsPathRooted(arg))
            {
                arg = Path.Combine(WorkingDirectory, arg);
                arg = Path.GetFullPath(arg);
            }

            return arg;
        }

        #endregion
    }
}
