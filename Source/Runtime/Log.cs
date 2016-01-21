using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf2Html5
{
    /// <summary>
    /// Serves as a common log sink for the converter procedure.
    /// </summary>
    public static class Log
    {
        #region Private

        enum Severity
        {
            information,
            warning,
            error
        }

        private static object _consolelock = new object();

        #endregion

        #region Trace Flags

        public static bool ShowTypeSystem = false;
        public static bool ShowCodeGeneration = false;
        public static bool ShowTypeResolution = false;
        public static bool ShowVerbose = false;
        public static bool ShowClassDependencies = false;
        public static bool ShowConverterModel = false;
        public static bool ShowHtmlConverter;
        public static bool ShowResources = false;
        public static bool ShowTemplates = false;
        public static bool ShowScripts = false;
        public static bool ShowDataTemplateHtml;
        public static bool ShowProfileLoader = false;

        #endregion

        /// <summary>
        /// Writes a log string.
        /// </summary>
        /// <param name="format">The format specification.</param>
        /// <param name="args">The arguments.</param>
        public static void Trace(string format, params object[] args)
        {
            Emit(Severity.information, format, args);
        }

        /// <summary>
        /// Writes a log string.
        /// </summary>
        /// <param name="format">The format specification.</param>
        /// <param name="args">The arguments.</param>
        public static void Warning(string format, params object[] args)
        {
            Emit(Severity.warning, "warning: " + format, args);
        }

        /// <summary>
        /// Writes a log string.
        /// </summary>
        /// <param name="format">The format specification.</param>
        /// <param name="args">The arguments.</param>
        public static void Error(string format, params object[] args)
        {
            Emit(Severity.error, "error: " + format, args);
        }

        #region Private Methods

        private static void Emit(Severity s, string format, params object[] args)
        {
            lock (_consolelock)
            {
                var color = Console.ForegroundColor;
                switch (s)
                {
                    case Severity.information:
                        // Console.ForegroundColor = ConsoleColor.White;
                        break;

                    case Severity.warning:
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        break;

                    case Severity.error:
                        Console.ForegroundColor = ConsoleColor.Red;
                        break;
                }


                var msg = string.Format(format, args);
                if (Debugger.IsAttached)
                {
                    Debug.WriteLine(msg);
                }

                Console.WriteLine(msg);
                Console.ForegroundColor = color;
            }
        }

        #endregion
    }
}
