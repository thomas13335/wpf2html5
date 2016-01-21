using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Wpf2Html5;
using Wpf2Html5.Converter;

namespace Wpf2Html5
{
    /// <summary>
    /// Converts a C# project into HTML5/JS.
    /// </summary>
    public class ProjectWpfToHtmlConverter : WpfToHtmlConverter
    {
        #region Private Fields

        private string _sourcedirectory;

        #endregion

        public event LoadSourceEventHandler LoadSources;

        #region Public Methods

        public void SetSourceDirectory(string path)
        {
            _sourcedirectory = path;
        }

        /// <summary>
        /// Converts a MSBUILD project.
        /// </summary>
        /// <param name="originalprojectfile"></param>
        public void AddProject(string originalprojectfile)
        {
            var projectfile = originalprojectfile;

            if(null == _sourcedirectory)
            {
                throw new Exception("source directory must be set before adding projects.");
            }

            Trace("searching '{0}' in '{1}' ...", originalprojectfile, _sourcedirectory);

            // search for project file
            if (!Path.IsPathRooted(projectfile))
            {
                if (null == (projectfile = SearchProject(_sourcedirectory, projectfile)))
                {
                    throw new Exception("project file '" + originalprojectfile + "' was not found.");
                }
            }

            var projectdirectory = Path.GetFullPath(Path.GetDirectoryName(projectfile));
            Trace("found project file in '{0}' ...", projectdirectory);

            var pfp = new MsProjectFileParser();
            pfp.Load(projectfile);

            // load the project file into XML

            // extract properties ...
            var assemblyname = pfp.GetPropertyValue("AssemblyName");
            var rootnamespace = pfp.GetPropertyValue("RootNamespace");
            var outputpath = pfp.GetPropertyValue("OutputPath");

            Trace("assembly '{0}' from project '{1}' ...", assemblyname, projectfile);

            var assemblypath = Path.Combine(projectdirectory, outputpath, assemblyname + ".dll");

            // TODO: obtain the compiled assembly: this is using the assembly found along the Wpf2Html5 binary
            // not a satisfactory solution ... load build results.
            //var assembly = Assembly.LoadFrom(assemblypath);
            var assembly = LoadAssembly(assemblypath);

            Trace("assembly '{0}' added '{1}'.", assembly.GetName().Name, assembly.Location);

            // add to assemblies to be processed
            AddAssembly(assembly);

            // enable source translation on the root namespace
            AddSourceNamespace(rootnamespace, assembly);


            // process sources from the directory of the project file (conventionally).
            //AddSourceFiles(Path.GetDirectoryName(projectfile));

            AddProjectFile(projectfile);
        }

        #endregion

        #region Private Methods

        private void AddSourceFiles(string directory)
        {
            foreach (var file in Directory.GetFiles(directory, "*.cs"))
            {
                AddSourceFile(file);
            }
            
            // TODO: recurse? => sure

            // trigger load event
            if(null != LoadSources)
            {
                LoadSources(this, new LoadSourcesEventArgs { Directory = directory });
            }
        }

        private string SearchProject(string directory, string projectfile)
        {
            string test;
            test = Path.Combine(directory, projectfile);
            if(File.Exists(test))
            {
                return test;
            }
            else if(Directory.Exists(directory))
            {
                foreach(var subdirectory in Directory.GetDirectories(directory))
                {
                    if(null != (test = SearchProject(subdirectory, projectfile)))
                    {
                        return test;
                    }
                }
            }

            return null;
        }

        #endregion
    }
}
