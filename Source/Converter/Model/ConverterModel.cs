using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Resources;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Baml2006;
using System.Xaml;
using System.Xml;
using Wpf2Html5;
using Wpf2Html5.Converter;
using Wpf2Html5.Converter.Configuration;
using Wpf2Html5.Converter.Interface;
using Wpf2Html5.Exceptions;

namespace Wpf2Html5.Converter.Model
{
    /// <summary>
    /// Converts W2H5 projects, may service as a view model for a *direct viewer*.
    /// </summary>
    public class ConverterModel : DependencyObject
    {
        #region Private Fields

        /// <summary>Map of resources to be included in the project.</summary>
        private SortedList<string, ConverterOutput> _resources = new SortedList<string, ConverterOutput>(StringComparer.InvariantCultureIgnoreCase);
        private List<string> _sourcedirectories = new List<string>();
        private ConverterReport _report;

        #endregion

        #region Public Properties

        /// <summary>The actual WPF to HTML converter in use.</summary>
        public ProjectWpfToHtmlConverter Converter
        {
            get { return (ProjectWpfToHtmlConverter)GetValue(ConverterProperty); }
            private set { SetValue(ConverterProperty, value); }
        }

        public static readonly DependencyProperty ConverterProperty =
            DependencyProperty.Register("Converter", typeof(ProjectWpfToHtmlConverter), typeof(ConverterModel));

        /// <summary>The actual project being processed.</summary>
        /// <remarks>Settings this property triggers the conversion process.</remarks>
        public ProjectSettings Project
        {
            get { return (ProjectSettings)GetValue(ProjectProperty); }
            set { SetValue(ProjectProperty, value); }
        }

        public static readonly DependencyProperty ProjectProperty =
            DependencyProperty.Register("Project", typeof(ProjectSettings), typeof(ConverterModel));

        /// <summary>Directory to receive output.</summary>
        public string OutputDirectory
        {
            get { return (string)GetValue(OutputDirectoryProperty); }
            set { SetValue(OutputDirectoryProperty, value); }
        }

        public static readonly DependencyProperty OutputDirectoryProperty =
            DependencyProperty.Register("OutputDirectory", typeof(string), typeof(ConverterModel));

        public bool InlineAllScript
        {
            get { return (bool)GetValue(InlineAllScriptProperty); }
            set { SetValue(InlineAllScriptProperty, value); }
        }

        public static readonly DependencyProperty InlineAllScriptProperty =
            DependencyProperty.Register("InlineAllScript", typeof(bool), typeof(ConverterModel));


        public string[] IncludeResources
        {
            get { return (string[])GetValue(IncludeResourcesProperty); }
            set { SetValue(IncludeResourcesProperty, value); }
        }

        public static readonly DependencyProperty IncludeResourcesProperty =
            DependencyProperty.Register("IncludeResources", typeof(string[]), typeof(ConverterModel));

        /// <summary>Type of the view model for the main XAML component.</summary>
        public Type ModelType
        {
            get { return (Type)GetValue(ModelTypeProperty); }
            set { SetValue(ModelTypeProperty, value); }
        }

        public static readonly DependencyProperty ModelTypeProperty =
            DependencyProperty.Register("ModelType", typeof(Type), typeof(ConverterModel));

        public bool IsOptimized
        {
            get { return (bool)GetValue(IsOptimizedProperty); }
            set { SetValue(IsOptimizedProperty, value); }
        }

        public static readonly DependencyProperty IsOptimizedProperty =
            DependencyProperty.Register("IsOptimized", typeof(bool), typeof(ConverterModel));


        public string PageTitle
        {
            get { return (string)GetValue(PageTitleProperty); }
            set { SetValue(PageTitleProperty, value); }
        }

        public static readonly DependencyProperty PageTitleProperty =
            DependencyProperty.Register("PageTitle", typeof(string), typeof(ConverterModel));



        public string TraceOutputFile
        {
            get { return (string)GetValue(TraceOutputFileProperty); }
            set { SetValue(TraceOutputFileProperty, value); }
        }

        public static readonly DependencyProperty TraceOutputFileProperty =
            DependencyProperty.Register("TraceOutputFile", typeof(string), typeof(ConverterModel));
        

        #endregion

        #region Diagnostics

        protected void Trace(string format, params object[] args)
        {
            ConverterReport.Trace(format, args);

            if (Log.ShowConverterModel)
            {
                Log.Trace(format, args);
            }
        }

        protected void Information(string format, params object[] args)
        {
            ConverterReport.Trace(format, args);

            Log.Trace(format, args);
        }

        protected void Warning(string format, params object[] args)
        {
            Log.Warning(format, args);
        }

        #endregion

        #region Overrides

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property == ProjectProperty)
            {
                LoadProject();
            }
        }

        #endregion

        #region Protected Methods

        /// <summary>Loads and converts the project.</summary>
        protected void LoadXaml()
        {
            if (null == Project)
            {
                throw new InvalidOperationException("project must be set.");
            }

            Converter = new ProjectWpfToHtmlConverter();
            Converter.SourceDirectory = Project.SourceDirectory;
            Converter.AssemblyAdded += (sender, e) => OnAssemblyAdded(e.LoadedAssembly);
            Converter.Indent = true;
            Converter.InlineAllScript = InlineAllScript;
            Converter.IsOptimized = IsOptimized;
            Converter.PageTitle = PageTitle;

            Converter.LoadSources += BindToOriginalSources;

            // load framework settings first
            var framework = Project.Framework;
            if (null != framework)
            {
                Trace("loading framework '{0}' ...", framework.Name);
                LoadProjectItems(Project.Framework);
            }
            else
            {
                // framework not used, register stock resources ...
                ExtractResources(typeof(Stock).Assembly);

                SetResourcesIncluded("base.css");
            }

            // project infos
            LoadProjectItems(Project);

            // check model type
            // configure the view model type for the selected XAMLSource
            if (Project.ModelType is Type)
            {
                ModelType = Project.ModelType as Type;
            }
            else if (Project.ModelType is String)
            {
                var qn = Project.ModelType.ToString()
                    .Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToArray();

                var tasm = Converter.Assemblies.Where(a => a.GetName().Name == qn[1]).FirstOrDefault();
                if(null == tasm)
                {
                    throw new Exception("assembly '" + qn[1] + "' was not found.");
                }

                ModelType = tasm.GetType(qn[0]);
                // ModelType = ObjectFactory.ResolveType(Project.ModelType.ToString());
            }
            else
            {
                ConverterReport.Warning("view model type [{0}] unavailable.", Project.ModelType);
            }


            Converter.DataContext = ModelType;
            Converter.ScriptSource = GetResource;

            // TODO: get desired XAML resource (must be embedded resource?)

            Uri baseuri;
            if (Uri.TryCreate(Project.XamlBaseUri, UriKind.RelativeOrAbsolute, out baseuri))
            {
                Converter.BaseUri = baseuri;
            }

            Trace("converter base URI: {0}", Converter.BaseUri);

            if (null != Project.XAMLSource)
            {
                var main = GetResourceConverter(Project.XAMLSource);
                var wpf = Converter.Convert(main, "index");
                OnXAMLLoaded(wpf);
            }
            else
            {
                Converter.ConvertToScript("script");
            }


            // read the outputs?
            var primaryoutput = Converter.Outputs.FirstOrDefault();
            if (null == primaryoutput)
            {
                Log.Warning("no primary output was generated.");
            }
            else
            {
                primaryoutput.IsIncluded = true;
                AddResource(primaryoutput);

                if (null != TraceOutputFile)
                {
                    var outfile = TraceOutputFile;
                    if (!Path.IsPathRooted(outfile)) outfile = Path.Combine(OutputDirectory, outfile);
                    using (var fout = File.Create(outfile))
                    {
                        Trace("writing trace output file.");
                        primaryoutput.GetStream().CopyTo(fout);
                    }
                }

                OnHTMLGenerated();

                ConverterReport.Trace("converter completed.");
            }
        }

        private void SetResourcesIncluded(params string[] rlist)
        {
            foreach (var r in rlist)
            {
                ConverterOutput output;
                if (_resources.TryGetValue(r, out output))
                {
                    output.IsIncluded = true;
                }
                else
                {
                    throw new ConverterException(ErrorCode.ResourceNotFound, "resource '" + r + "' was not found.");
                }
            }
        }

        /// <summary>Called when the converter has processed a CSPROJ; bind resources to their local files,
        /// so that edit/refresh can be used instead restarting the testbed.</summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BindToOriginalSources(object sender, LoadSourcesEventArgs e)
        {
            BindToOriginalSourcesRecurse(e.Directory);
        }

        private void BindToOriginalSourcesRecurse(string directory)
        {
            foreach (var file in Directory.GetFiles(directory))
            {
                if (IsEditableResource(file))
                {
                    var name = Path.GetFileName(file);
                    ConverterOutput output;
                    if(_resources.TryGetValue(name, out output))
                    {
                        output.OriginalPath = file;

                        //ConverterReport.Trace("resource {0} ==> {1}", name, file);
                    }
                }
            }

            foreach(var subdir in Directory.GetDirectories(directory))
            {
                BindToOriginalSourcesRecurse(subdir);
            }
        }

        protected virtual void OnXAMLLoaded(object wpf)
        { }

        protected virtual void OnHTMLGenerated()
        { }

        protected virtual void OnTerminated()
        { }

        private bool IsEditableResource(string name)
        {
            var ext = Path.GetExtension(name);
            switch(ext)
            {
                case ".css":
                case ".js":
                case ".png":
                    return true;

                default:
                    return false;
            }
        }

        protected virtual ConverterOutput GetResourceConverter(string arg)
        {
            ConverterOutput output;
            _resources.TryGetValue(arg, out output);
            return output;
        }

        /// <summary>
        /// Retrieves a resource stream from the resources collection.
        /// </summary>
        /// <param name="arg">The name or the resource, without any path.</param>
        /// <returns></returns>
        protected virtual Stream GetResource(string arg)
        {
            Stream result = null;
            ConverterOutput output;
            if (_resources.TryGetValue(arg, out output))
            {
                if (null != output.OriginalPath && IsEditableResource(arg))
                {
                    // Trace("reading original resource '{0}' ...", output.OriginalPath);
                    result = File.OpenRead(output.OriginalPath);
                }
                else
                {
                    result = output.GetStream();
                }
            }

            if(null == result)
            {
                throw new ConverterException(ErrorCode.ResourceNotFound, "resource '" + arg + "' was not found.");
            }

            return result;
        }

        #endregion

        #region Private Methods

        private void LoadProject()
        {
            // clear any existing converter
            Converter = null;

            if (null == Project)
            {
                return;
            }

            Trace("loading project '{0}' ...", Project.Name);

            if(null == OutputDirectory)
            {
                Warning("output directory is not set.");
            }

            using (_report = new ConverterReport())
            {
                Trace("load master project '{0}' ...", Project.Name);

                // clear collection of source directories
                _sourcedirectories.Clear();

                if (null == Project.XAMLSource)
                {
                    ConverterReport.Warning("[XOZXWDT5TD] XAML source is not set.");
                }

                // convert the main XAML source ...
                LoadXaml();

                GenerateResourceAssembly();
            }
        }

        private void OnAssemblyAdded(Assembly assembly)
        {
            ExtractResources(assembly);
        }

        private void ExtractResources(Assembly assembly)
        {
            Trace("extract resources from assembly '{0}' ...", assembly.Location);

            foreach (var resname in assembly.GetManifestResourceNames())
            {
                if (resname.EndsWith(".g.resources"))
                {
                    using(var stream = assembly.GetManifestResourceStream(resname))
                    using (ResourceReader reader = new ResourceReader(stream))
                    {
                        foreach (DictionaryEntry entry in reader)
                        {
                            if (entry.Key is string)
                            {
                                var name = (string)entry.Key;
                                var extension = Path.GetExtension((string)name);
                                if (name.EndsWith(".baml"))
                                {
                                    // Trace("gresource: {0}", entry.Key);
                                    ExtractBAML(name, entry.Value as Stream);
                                }
                                else if(name.EndsWith(".xaml"))
                                {
                                    AddResource(name, (Stream)entry.Value);
                                }
                            }
                        }
                    }
                }
                else if(resname.EndsWith(".resources"))
                {

                }
                else
                {
                    // to be determines resource file name
                    string name;

                    var parts = resname.Split('.');
                    var count = parts.Length;
                    if (count > 2)
                    {
                        var lastwo = parts.Skip(count - 2).ToArray();
                        name = lastwo[0] + "." + lastwo[1];
                    }
                    else
                    {
                        name = resname;
                    }

                    AddResource(name, assembly.GetManifestResourceStream(resname));
                }
            }
        }

        private void AddResource(string name, Stream stream)
        {
            var content = new ConverterOutput(stream)
            {
                Name = name,
                Disposition = OutputDisposition.input
            };

            AddResource(content);
        }

        private void AddResource(ConverterOutput output)
        {
            if (Log.ShowResources)
            {
                Log.Trace("add resource {0} => {1}", output.Name, output);
            }

            _resources[output.Name] = output;
        }

        private void ExtractBAML(string name, Stream stream)
        {
            var reader = new Baml2006Reader(stream);
            var settings = new XamlObjectWriterSettings ();

            settings.BeforePropertiesHandler += (sender, e) =>
            {
                Trace("properties for {0}", e.Instance);
            };

            settings.XamlSetValueHandler += (sender, e) =>
            {
                //TraceTarget.Trace("set value {0} = {1}", e.Member.Name, e.Value);

                /*if (e.Member.Name == "DeferrableContent")
                {
                    e.Handled = true;
                }*/
            };

            var writer = new XamlObjectWriter(reader.SchemaContext, settings);
            while (reader.Read())
            {
                writer.WriteNode(reader);
            }
            var x = writer.Result;

            var ms = new MemoryStream();
            using (var xwriter = XmlWriter.Create(ms, new XmlWriterSettings { Indent = true }))
            {
                System.Windows.Markup.XamlWriter.Save(x, xwriter);
            }

            var xamlname = Path.ChangeExtension(name, ".xaml");

            ms.Position = 0;
            AddResource(xamlname, ms);
        }

        /// <summary>Loads project items from the settings.</summary>
        /// <param name="project"></param>
        private void LoadProjectItems(ProjectSettings project)
        {
            var isframework = project is FrameworkSettings;

            Trace("load directory '{0}' ...", project.ProjectDirectory);

            // additional assemblies
            if (null != project.AdditionalAssemblies)
            {
                foreach (var name in project.AdditionalAssemblies)
                {
                    Converter.LoadAssembly(name);
                }
            }

            // path to source
            Converter.SetSourceDirectory(project.SourceDirectory);

            // projects
            if (null != project.Projects)
            {
                foreach (var csproject in project.Projects)
                {
                    Trace("  add source project '{0}' ...", csproject);
                    Converter.AddProject(csproject);
                }
            }

            if(null != project.SourceFiles)
            {
                foreach(var csfile in project.SourceFiles)
                {
                    var path = csfile;
                    if(!Path.IsPathRooted(path))
                    {
                        path = Path.GetFullPath(Path.Combine(project.SourceDirectory, path));
                    }

                    Trace("  add source file '{0}' ...", path);
                    Converter.AddSourceFile(path);
                }
            }

            // script includes
            AddScripts(project.FrameworkScripts, ScriptOptions.Framework);
            AddScripts(project.ScriptsBefore, ScriptOptions.Before);
            AddScripts(project.ScriptsAfter, ScriptOptions.After);

            // CSS
            if(null != project.Resources)
            {
                AddProjectResources(project);
            }

            // external scripts
            if(null != project.ExternalScripts)
            {
                foreach(var e in project.ExternalScripts)
                {
                    Converter.AddScript(e);
                }
            }
        }

        private void AddProjectResources(ProjectSettings project)
        {
            var list = new List<string>();

            foreach (var s in project.Resources)
            {
                if (s.Contains('*') || s.Contains('?'))
                {
                    var wc = WildcardFactory.BuildWildcards(s);

                    foreach (var r in _resources.Where(e => wc.IsMatch(e.Key)).Select(e => e.Key))
                    {
                        list.Add(r);
                    }
                }
                else
                {
                    list.Add(s);
                }
            }

            foreach (var s in list)
            {
                SetResourcesIncluded(new[] { s });

                if (s.EndsWith(".css"))
                {
                    // TODO: deuglify
                    Converter.AddCSS(new string[] { s });
                }
            }
        }

        private void AddScripts(IEnumerable<string> names, ScriptOptions options)
        {
            if (null != names)
            {
                foreach (var script in names)
                {
                    Converter.AddScript(script, options);
                }
            }
        }

        /// <summary>
        /// Generates a resource assembly from the resources related to this project.
        /// </summary>
        private void GenerateResourceAssembly()
        {
            if (null == OutputDirectory)
            {
                Warning("output directory is not set, not generating assembly.");
                return;
            }

            if(null == Project.TargetName)
            {
                Warning("project 'TargetName' is not set, not generating assembly.");
                return;
            }

            var targetname = Project.TargetName;
            var targetdir = Path.Combine(OutputDirectory);

            Trace("generating resource assembly '{0}' into '{1}' ...", targetname, targetdir);

            // wildcard for objects to include
            Regex wildcard = null;
            if (null != IncludeResources)
            {
                wildcard = WildcardFactory.BuildWildcards(IncludeResources);
            }
            else
            {
                wildcard = new Regex(".*");
            }

            // construct the assembly name
            var assemblyname = new AssemblyName(targetname);
            var filename = assemblyname.Name + ".dll";

            // create assembly builder ...
            var assemblybuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyname, AssemblyBuilderAccess.Save, targetdir);
            var modulebuilder = assemblybuilder.DefineDynamicModule(assemblyname.Name, filename, false);
            int numresources = 0;

            foreach(var pair in _resources.Where(e => e.Value.IsIncluded))
            {
                var output = pair.Value;
                if (output.Disposition == OutputDisposition.output
                    || wildcard.IsMatch(output.Name))
                {
                    var stream = output.GetStream();

                    if (Log.ShowResources)
                    {
                        Log.Trace("  {0,-30} {1,-10} {2,10} {3}", output.Name, output.Disposition, output.FileSize, output.OriginalPath);
                    }

                    modulebuilder.DefineManifestResource(pair.Key, stream, ResourceAttributes.Public);
                    ++numresources;
                }
            }

            assemblybuilder.Save(filename, PortableExecutableKinds.Required32Bit, ImageFileMachine.I386);

            var targetpath = Path.Combine(targetdir, filename);

            Information("generated assembly '{0}' with {1} resources.", targetpath, numresources);
        }

        #endregion

    }
}
