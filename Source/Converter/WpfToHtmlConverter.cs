using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Xaml;
using System.Xml;
using Wpf2Html5.Builder;
using Wpf2Html5.Converter;
using Wpf2Html5.Converter.Framework;
using Wpf2Html5.Converter.Interface;
using Wpf2Html5.Exceptions;
using Wpf2Html5.Factory;
using Wpf2Html5.TypeSystem;
using Wpf2Html5.TypeSystem.Interface;

namespace Wpf2Html5
{
    /// <summary>
    /// Converts a WPF application into native HTML5/JS
    /// </summary>
    /// <remarks>
    /// <para>The conversion process is triggered by inserting XAML source into the converter.
    /// Before that, view model types can be registered. By specifing the source code,
    /// view model types are converted into JScript code.
    /// </para>
    /// </remarks>
    public class WpfToHtmlConverter : ConverterStack, IConverterContext
    {
        #region Private Fields

        private IModule _module;
        private CSharpToJScriptConverter _cs2js;
        private JScriptBuilder _initializationbuilder = new JScriptBuilder();
        private JScriptBuilder _scriptincludes = new JScriptBuilder();
        private JScriptBuilder _resizebuilder = new JScriptBuilder();
        private int _idseed = 0;
        private bool _codegenerated = false;
        private List<string> _frameworkscripts = new List<string>();
        private List<string> _scriptsbefore = new List<string>();
        private List<string> _scriptsafter = new List<string>();
        private List<string> _cssscripts = new List<string>();
        private List<ExternalScriptReference> _externalscripts = new List<ExternalScriptReference>();

        private Dictionary<string, IControlConverter> _controls = new Dictionary<string, IControlConverter>();
        private Dictionary<string, ConverterOutput> _outputs = new Dictionary<string, ConverterOutput>();
        private Type _dc;
        private Dictionary<string, DataTemplateInfo> _datatemplates = new Dictionary<string, DataTemplateInfo>();
        private List<DataTemplateInfo> _templatesinorder = new List<DataTemplateInfo>();
        private int _errorcount;
        private int _warningcount;

        #endregion

        #region Public Properties

        public string SourceDirectory { get; set; }

        public JScriptBuilder Declarations { get { return _cs2js.Declarations; } }

        public JScriptBuilder CodeBuilder { get { return _initializationbuilder; } }

        public IEnumerable<ConverterOutput> Outputs { get { return _outputs.Values; } }

        public ITypeContext Module { get { return _module; } }

        public bool Indent { get; set; }

        public bool InlineAllScript { get; set; }

        public bool IsCompact { get; set; }

        public Type DataContext { get { return _dc; } set { SetDataContext(value); } }

        public Func<string, Stream> ScriptSource { get; set; }

        public string PageTitle { get; set; }

        public bool IsOptimized { get; set; }

        public Uri BaseUri { get; set; }

        public IEnumerable<System.Reflection.Assembly> Assemblies { get { return _cs2js.Assemblies; } }

        public int ErrorCount { get { return _errorcount; } }

        public int WarningCount { get { return _warningcount; } }

        public ConverterOutput CurrentResource { get; private set; }

        #endregion

        #region Events

        public event AssemblyLoadEventHandler AssemblyAdded;

        #endregion

        #region Construction

        /// <summary>Constructs a new WPF to HTML converter.</summary>
        public WpfToHtmlConverter()
        {
            PageTitle = "Wpf2Html5 Generated Page";
        }

        #endregion

        #region Diagnostics

        protected static void Trace(string format, params object[] args)
        {
            if (Log.ShowHtmlConverter)
            {
                Log.Trace("w2h5: " + format, args);
            }
        }

        protected static void Warning(string format, params object[] args)
        {
            Log.Warning(format, args);
        }

        protected static void Information(string format, params object[] args)
        {
            Log.Trace(format, args);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a single C# source file to the context.
        /// </summary>
        /// <param name="filename"></param>
        /// <remarks>The source file is parsed an the corresponding JScript code is generated.</remarks>
        public void AddSourceFile(string filename)
        {
            InitializeModule();

            _cs2js.AddSource(filename);
        }

        /// <summary>
        /// Adds a MSBUILD project to the context.
        /// </summary>
        /// <param name="projectfile"></param>
        public void AddProjectFile(string projectfile)
        {
            InitializeModule();

            _cs2js.AddProjectFile(projectfile);
        }

        public void AddSourceNamespace(string ns, System.Reflection.Assembly assembly)
        {
            if (string.IsNullOrEmpty(ns)) throw new ArgumentNullException("ns");
            if (null == assembly) throw new ArgumentNullException("assembly");

            InitializeModule();
            AddAssembly(assembly);
            _module.AddSourceNamespace(ns, assembly);
        }

        /// <summary>Adds an assembly for type lookup.</summary>
        /// <param name="assembly"></param>
        public void AddAssembly(System.Reflection.Assembly assembly)
        {
            InitializeModule();

            if(_cs2js.AddAssembly(assembly))
            {
                Trace("assembly to convert '{0}' added.", assembly.FullName);

                if (null != AssemblyAdded)
                {
                    AssemblyAdded(this, new AssemblyLoadEventArgs(assembly));
                }
            }
        }

        public Assembly LoadAssembly(string name)
        {
            Assembly assembly;
            if (name.Contains('/') || name.Contains('\\') || name.EndsWith(".dll"))
            {
                var path = name;
                if (!Path.IsPathRooted(path)) path = Path.Combine(SourceDirectory, name);
                path = Path.GetFullPath(path);
                assembly = Assembly.LoadFrom(path);

                Trace("assembly '{0}' loaded from path '{1}'.", assembly.FullName, path);
            }
            else
            {
                assembly = Assembly.Load(name);
            }

            AddAssembly(assembly);

            return assembly;
        }

        public void AddScript(string filename, ScriptOptions options)
        {
            switch(options)
            {
                case ScriptOptions.Framework:
                    _frameworkscripts.Add(filename);
                    break;

                case ScriptOptions.Before:
                    _scriptsbefore.Add(filename);
                    break;

                case ScriptOptions.After:
                    _scriptsafter.Add(filename);
                    break;

                default:
                    throw new ArgumentException("invalid script option specified.");
            }
        }

        public void AddCSS(string[] filenames)
        {
            _cssscripts.AddRange(filenames);
        }

        public void AddScript(ExternalScriptReference escript)
        {
            _externalscripts.Add(escript);
        }

        private bool ProcessException(Exception error)
        {
            if (error is RewriterException)
            {
                var ex = (RewriterException)error;
                var ls = ex.Location.GetLineSpan();
                var line = ls.Span.Start.Line + 1;
                var col = ls.Span.Start.Character + 1;
                var iserror = ex.Code.IsError();
                var tag = iserror ? "error" : "warning";
                Log.Trace("{0}({1},{2}): {3}: {4}", ls.Path, line, col, tag, ex.Message);

                if (!iserror)
                {
                    _warningcount++;
                }
                else
                {
                    _errorcount++;
                }

                return true;
            }
            else if (error is UnresolvedTypeException)
            {
                Log.Warning("{0}", error.Message);

                _errorcount++;

                return true;
            }
            else
            {
                Log.Error("{0}", error.Message);

                _errorcount++;
                return false;
            }
        }

        public object Convert(ConverterOutput resource, string name = null)
        {
            try
            {
                CurrentResource = resource;

                return Convert(name ?? resource.Name, resource.GetStream());
            }
            catch (Exception ex)
            {
                ProcessException(ex);
                return null;
            }
            finally
            {
                CurrentResource = null;
            }
        }

        public object Convert(string xamltext, XmlWriter writer)
        {
            var ms = new MemoryStream();
            using (var swriter = new StreamWriter(ms, Encoding.UTF8, 10000, true))
            {
                swriter.Write(xamltext);
            }

            ms.Position = 0;
            return Convert(ms, writer);
        }

        /// <summary>
        /// Converts a XAML resource to HTML.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="xamltext"></param>
        /// <returns></returns>
        public object Convert(string name, Stream stream)
        {
            InitializeModule();

            // check not already exists output
            var filename = name + ".html";
            if (_outputs.ContainsKey(filename))
            {
                throw new Exception("output '" + filename + "' already exists.");
            }

            object result;
            var ms = new MemoryStream();
            using (var writer = XmlWriter.Create(ms, new XmlWriterSettings { Indent = Indent, OmitXmlDeclaration = true }))
            {
                writer.WriteDocType("HTML", null, null, null);
                result = Convert(stream, writer);
            }

            // add to outputs
            ms.Position = 0;
            _outputs[filename] = new ConverterOutput(ms)
            {
                Name = filename,
                Disposition = OutputDisposition.output
            };

            return result;
        }

        /// <summary>
        /// Converts a XAML definition to HTML and returns the resulting WPF control.
        /// </summary>
        /// <param name="reader">XML input</param>
        /// <param name="writer">HTML output</param>
        /// <returns>The resulting WPF control object.</returns>
        public object Convert(Stream input, XmlWriter writer)
        {
            InitializeModule();

            var pc = new ParserContext();
            pc.BaseUri = BaseUri;

            Log.Trace("processing XAML ...");
            var obj = System.Windows.Markup.XamlReader.Load(input, pc);

            EmitHtmlDocument(writer, obj);

            return obj;
        }

        public void ConvertToScript(string name)
        {
            var filename = name + ".js";
            if (_outputs.ContainsKey(filename))
            {
                throw new Exception("output '" + filename + "' already exists.");
            }

            var ms = new MemoryStream();
            using (var writer = XmlWriter.Create(ms, new XmlWriterSettings { Indent = Indent, OmitXmlDeclaration = true }))
            {
                writer.WriteStartElement("head");
                GenerateCode();
                EmitScript(writer);
                writer.WriteEndElement();
            }

            ms.Position = 0;
            _outputs[filename] = new ConverterOutput(ms)
            {
                Name = filename,
                Disposition = OutputDisposition.output
            };
        }


        public void GenerateCode()
        {
            if (!_codegenerated)
            {
                _cs2js.PrepareCode();

                Log.Trace("generating code ...");

                if (Log.ShowClassDependencies)
                {
                    // Log.Trace("framework datatypes:\n{0}", Wpf2Html5.TypeSystem.TypeSystem.GetInstance().ToDebugString());
                    Log.Trace("module data types: \n{0}", _module.ToDebugString(0, true));
                }

                _cs2js.GenerateCode();

                _codegenerated = true;
            }
        }

        #endregion

        #region Initialization

        protected void InitializeModule()
        {
            if (null == _module)
            {
                _module = ModuleFactory.CreateModule("application");

                // interfer with type conversion decision JL3A77746Z.
                _module.ShouldTranslateType = ShouldTranslateType;
                _cs2js = new CSharpToJScriptConverter(_module);
                _cs2js.Error += OnCodeConverterError;
                _cs2js.AddAssembly(GetType().Assembly);
            }
        }

        private void OnCodeConverterError(object sender, ConverterErrorEventArgs e)
        {
            e.Handled = ProcessException(e.Error);
        }

        #endregion

        #region Settings

        class Settings
        {
            public const string HTMLNamespaceURI = "http://www.w3.org/1999/xhtml";
            public const bool Indent = false;
        }

        #endregion

        #region HTML

        private void EmitHtmlHead(XmlWriter writer)
        {
            writer.WriteStartElement("head");
            writer.WriteStartElement("title");
            writer.WriteString(PageTitle);
            writer.WriteEndElement();

            //<meta name="viewport" content="width=device-width, initial-scale=1">
            writer.WriteStartElement("meta");
            writer.WriteAttributeString("name", "viewport");
            writer.WriteAttributeString("content", "width=320, initial-scale=1, user-scalable=no");
            writer.WriteEndElement();

            // stylesheet
            EmitStylesheetRelation(writer, "base.css");

            foreach(var css in _cssscripts)
            {
                EmitStylesheetRelation(writer, css);
            }

            foreach(var escript in _externalscripts)
            {
                writer.WriteStartElement("script");
                writer.WriteAttributeString("src", escript.URL);
                if (escript.Defer) { writer.WriteAttributeString("defer", "true"); }
                if (escript.Async) { writer.WriteAttributeString("async", "true"); }
                writer.WriteString(" ");
                writer.WriteEndElement();
            }
            
            writer.WriteEndElement();
        }

        private void EmitStylesheetRelation(XmlWriter writer, string cssname)
        {
            writer.WriteStartElement("link");
            writer.WriteAttributeString("rel", "stylesheet");
            writer.WriteAttributeString("type", "text/css");
            writer.WriteAttributeString("href", cssname);
            writer.WriteEndElement();
        }

        private void EmitHtmlDocument(XmlWriter writer, object obj)
        {
            writer.WriteStartElement("html");
            EmitHtmlHead(writer);
            writer.WriteStartElement("body");

            // CWVYGXULNX: the 'idbody' identifier is used to refer to the topmost element.
            writer.WriteAttributeString("id", "idbody");

            (this as IConverterContext).Convert(obj, writer);

            // generate code after control conversion, may add script dependencies
            GenerateCode();
            EmitDataContextInitialization();

            // emit template factories
            foreach(var info in _templatesinorder)
            {
                EmitRegisterDataTemplate(info);
                Declarations.Write(info.FactoryCode);
            }

            EmitScript(writer);

            writer.WriteEndElement();
            writer.WriteEndElement();
        }


        #endregion

        #region Script

        private void EmitScriptIncludesBefore(XmlWriter writer)
        {
            EmitScriptItems(writer, _frameworkscripts);
            EmitScriptItems(writer, _scriptsbefore);
        }

        private void EmitScriptIncludesAfter(XmlWriter writer)
        {
            EmitScriptItems(writer, _scriptsafter);
        }

        private void EmitScriptItems(XmlWriter writer, IEnumerable<string> scriptincludes)
        {
            if (!InlineAllScript)
            {
                foreach (var include in scriptincludes)
                {
                    writer.WriteStartElement("script");
                    writer.WriteAttributeString("src", include);
                    writer.WriteString("\u00A0");
                    writer.WriteEndElement();
                }
            }
            else
            {
                var common = new JScriptWriter();

                if(null == ScriptSource)
                {
                    throw new Exception("script source required to inline scripts.");
                }

                foreach (var include in scriptincludes)
                {
                    using(var reader = new StreamReader(ScriptSource(include)))
                    {
                        Log.Trace("including script '{0}' ...", include);

                        var jscode = reader.ReadToEnd();
                        common.WriteLine(jscode);
                    }
                }

                // compactify code ...
                var compacter = new JScriptBuilder();
                compacter.IsCompact = IsCompact;
                compacter.RewriteProgram(common);

                _scriptincludes.Write(compacter);
            }
        }

        private void EmitScript(XmlWriter writer)
        {
            EmitScriptIncludesBefore(writer);

            var code = new JScriptWriter();
            code.WriteLine();
            code.WriteLine("// generated");
            code.Write(_scriptincludes);
            code.Write(Declarations);

            // consumed, more to come ...

            // resize handler function (unused currently)
            code.WriteLine("function docResized() ");
            code.EnterBlock();
            code.WriteLine("ResizeManager_Initialize();");
            code.Write(_resizebuilder);
            code.LeaveBlock();

            // bindings initialization
            code.WriteLine();
            code.WriteLine("function Bindings_Initialize() ");
            code.EnterBlock();
            code.WriteLine("if(window['BindingObject'] !== undefined)");
            code.EnterBlock();
            code.Write(CodeBuilder);
            code.LeaveBlock();
            code.LeaveBlock();

            // primary document ready handler
            code.WriteLine();
            code.WriteLine("docReady(function() ");
            code.EnterBlock();
            code.WriteLine("DataContext_Initialize(function() { Bindings_Initialize(); });");
            code.WriteLine("docResized();");
            code.WriteLine("DragDrop_Initialize();");
            code.LeaveBlock();
            code.WriteLine(");");


            // TraceTarget.Trace("jscript code in HTML:\n{0}\n\n", code.Text);

            writer.WriteStartElement("script");

            if (!IsOptimized)
            {

                writer.WriteRaw(code.Text);
            }
            else
            {
                Information("generating optimized code ...");
                var builder = new JScriptBuilder();
                builder.SuppressedGlobalFunctions.Add("trace");
                builder.RewriteProgram(code);
                writer.WriteRaw(builder.Text);
            }

            writer.WriteEndElement();

            // application specific initializer scripts ...
            _scriptincludes = new JScriptBuilder();
            EmitScriptIncludesAfter(writer);

            if (_scriptincludes.Text.Length > 0)
            {
                writer.WriteStartElement("script");
                writer.WriteRaw(_scriptincludes.Text);
                writer.WriteEndElement();
            }
        }

        #endregion

        #region IConverterContext

        /// <summary>
        /// Allocates a unique identifier for a control converter instance.
        /// </summary>
        /// <param name="converter">The object representing the control.</param>
        /// <returns>The newly allocated identifier.</returns>
        string IConverterContext.AllocateIdentifier(IControlConverter converter)
        {
            var prefix = converter.GetType().Name;
            var id = AllocateIdentifier(prefix);
            _controls[id] = converter;
            return id;
        }

        public IControlConverter GetControlByID(string id)
        {
            IControlConverter r;
            _controls.TryGetValue(id, out r);
            return r;
        }

        void IConverterContext.AddBinding(IControlConverter target, string prop, Binding b)
        {
            new BindingConverter(CodeBuilder).AddBinding(target.ID, prop, b);
        }

        void IConverterContext.AddKeyBinding(IControlConverter target, KeyBinding kb)
        {
            new BindingConverter(CodeBuilder).AddKeyBinding(target.ID, kb);
        }

        public void AddMouseBinding(IControlConverter target, MouseBinding mb)
        {
            new BindingConverter(CodeBuilder).AddMouseBinding(target.ID, mb);
        }

        IControlConverter IConverterContext.Convert(object obj, XmlWriter writer, IConverterContext context, ConverterArguments args)
        {
            return HtmlFactory.Convert(obj, context ?? (this as IConverterContext), writer, args);
        }

        public void AddSizeBinding(IControlConverter container, string suffix)
        {
            string js = container.ControlType.Name + "_" + suffix + "(" + container.ID + ");";
            _resizebuilder.WriteLine(js);
        }

        private void EmitRegisterDataTemplate(DataTemplateInfo info, string contextid = null, bool noresourcekey = false)
        {
            contextid = contextid ?? info.ContextID;

            string resourcekey = null;
            if (!noresourcekey)
            {
                resourcekey = info.ResourceKey as string;
            }

            if (Log.ShowTemplates)
            {
                Log.Trace("[EmitRegisterDataTemplate] {0,-30} {1,-30} {2,-20} {3:X8} -> {4}", info.TypeName, contextid, resourcekey, info.Control.GetHashCode(), info.Key);
            }

            Declarations.WriteLine("TemplateFactory.registertemplate(" +
                QuoteString(info.TypeName) + ", " +
                QuoteString(contextid) + ", " +
                QuoteString(resourcekey) + ", " +
                info.Key + ");");
        }

        public void RegisterDataTemplate(DataTemplate template, DataTemplateInfo info)
        {
            // _datatemplates[key] = info;
            _templatesinorder.Add(info);
        }

        public void GenerateDataTemplate(DataTemplate template, XmlWriter writer, IConverterContext context)
        {
            if (null == context)
            {
                context = this as IConverterContext;
            }

            // purportedly the only place where data templates get started ...
            context.Convert(template, writer);
        }

        public void TriggerItemReference(string name)
        {
            if(!_cs2js.ItemReferenced(name))
            {
                throw new ConverterException(ErrorCode.UnresolvedTypeName, "type '" + name + "' not found.");
            }
        }

        #endregion

        #region Private Methods

        private bool ShouldTranslateType(Type arg)
        {
            return arg.FullName == _cs2js.CurrentClassName;
        }

        private string AllocateIdentifier(string name)
        {
            return name + (++_idseed);

        }

        private void SetDataContext(Type value)
        {
            if (null != (_dc = value))
            {
                AddAssembly(_dc.Assembly);
            }
        }

        private string QuoteString(string s)
        {
            if (null == s) return "null";
            else return "\"" + s.Replace("\"", "\\\"") + "\"";
        }


        private void EmitDataContextInitialization()
        {
            if (null != DataContext)
            {
                // PXK3ZVWCJN: create the root datacontext object.
                Declarations.WriteLine("var datacontext = new " + DataContext.Name + "();");
            }
            else
            {
                Log.Warning("warning: no data context specified.");
            }
        }

        #endregion
    }
}
