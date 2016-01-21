using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace Wpf2Html5.Converter.Configuration
{
    /// <summary>
    /// Describes a converter project configuration, stored in JSON format.
    /// </summary>
    /// <remarks>
    /// <para>Path are resolved relative to the baseuri of the configuration file, 
    /// unless the source directory is specified.</para></remarks>
    public class ProjectSettings
    {
        #region Properties

        public string ProjectDirectory { get; set; }

        public string FrameworkDirectory { get; set; }

        public string Name { get; set; }

        public string PageTitle { get; set; }

        public string SourceDirectory { get; set; }

        /// <summary>
        /// Specifies the MSBUILD projects to process.
        /// </summary>
        public string[] Projects { get; set; }

        /// <summary>
        /// Specifies individual files to process.
        /// </summary>
        public string[] SourceFiles { get; set; }

        public string[] AdditionalAssemblies { get; set; }

        /// <summary>
        /// The DataType of the model.
        /// </summary>
        public object ModelType { get; set; }

        public string XAMLSource { get; set; }

        public string XamlBaseUri { get; set; }

        public string AdditionalSourceDirectories { get; set; }

        public string[] FrameworkScripts { get; set; }

        public string[] ScriptsBefore { get; set; }

        public string[] ScriptsAfter { get; set; }

        public string[] Resources { get; set; }

        /// <summary>
        /// Name of the assembly to be generated.
        /// </summary>
        public string TargetName { get; set; }

        public ExternalScriptReference[] ExternalScripts { get; set; }

        [JsonIgnore]
        public virtual FrameworkSettings Framework
        {
            get
            {
                if (null != FrameworkDirectory)
                {
                    string directory;
                    if (Path.IsPathRooted(FrameworkDirectory))
                    {
                        directory = FrameworkDirectory;
                    }
                    else
                    {
                        directory = Path.GetFullPath(Path.Combine(ProjectDirectory, FrameworkDirectory));
                    }

                    return new FrameworkSettings(directory);
                }
                else
                {
                    return null;
                }

            }
        }

        #endregion

        #region Construction

        public ProjectSettings(string projectdirectory)
        {
            Initialize(projectdirectory);
        }

        private void Initialize(string projectdirectory)
        {
            ProjectDirectory = projectdirectory;
            SourceDirectory = ProjectDirectory;

            /*if (null == XAMLSource)
            {
                XAMLSource = "View.xaml";
            }*/ 

            if (null == TargetName)
            {
                TargetName = "W2H5.Target";
            }

            if (null == PageTitle)
            {
                PageTitle = "[PageTitle]";
            }
        }

        #endregion

        #region Serialization

        public static ProjectSettings Load(string filepath)
        {
            ProjectSettings settings;
            var ext = Path.GetExtension(filepath);
            if (ext == ".xml")
            {
                settings = LoadXml(filepath);
            }
            else
            {
                var directory = Path.GetDirectoryName(filepath);

                using (var reader = new StreamReader(filepath))
                {
                    settings = JsonConvert.DeserializeObject<ProjectSettings>(reader.ReadToEnd());
                    settings.Initialize(directory);

                }
            }

            return settings;
        }

        public void Save(string filepath)
        {
            using(var writer = new StreamWriter(filepath))
            {
                writer.Write(JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented));
            }
        }

        private static ProjectSettings LoadXml(string filepath)
        {
            var directory = Path.GetDirectoryName(filepath);
            var doc = new XmlDocument();

            using (var reader = XmlReader.Create(filepath))
            {
                doc.Load(reader);
            }

            var settings = new ProjectSettings(directory);
            var ptype = settings.GetType();

            foreach (var e in doc.DocumentElement.ChildNodes.OfType<XmlElement>())
            {
                var pi = ptype.GetProperty(e.LocalName);
                if (null == pi)
                {
                    throw new Exception("property '" + e.LocalName + "' is not defined.");
                }

                var value = e.InnerText.Trim();

                if (pi.Name == "FrameworkDirectory")
                {
                    value = MakeAbsolutePath(directory, value);
                }

                if (pi.Name == "ModelType")
                {
                    settings.ModelType = CreateModelType(value);
                }
                else if (pi.PropertyType.IsArray)
                {
                    var a0 = (string[])(pi.GetValue(settings) ?? new string[0]);
                    string[] a1;

                    if (pi.Name == "AdditionalScripts")
                    {
                        var add = value
                            .Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(s => s.Trim());

                        a1 = a0.Union(add).ToArray();
                    }
                    else
                    {
                        a1 = a0.Union(new string[] { value }).ToArray();
                    }

                    pi.SetValue(settings, a1);
                }
                else if (pi.PropertyType == typeof(string))
                {
                    pi.SetValue(settings, value);
                }
                else
                {
                    throw new Exception("property '" + e.LocalName + "' type not supported.");
                }
            }

            return settings;
        }

        private static string MakeAbsolutePath(string directory, string value)
        {
            if (!Path.IsPathRooted(value))
            {
                value = Path.Combine(directory, value);
            }

            return Path.GetFullPath(value);
        }

        private static object CreateModelType(string value)
        {
            var parts = value.Split(new char[] { ',' }, 2, StringSplitOptions.RemoveEmptyEntries);

            Assembly assembly = null;

            // add assembly name supplied with qn ..
            if (parts.Length < 2)
            {
                throw new ArgumentException("fully qualified type name required.");
            }

            var aname = parts[1];
            assembly = Assembly.Load(new AssemblyName(aname));
            var type = assembly.GetType(parts[0]);

            return Activator.CreateInstance(type);
        }

        #endregion
    }
}
