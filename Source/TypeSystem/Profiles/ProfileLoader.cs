using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Wpf2Html5.Exceptions;
using Wpf2Html5.TypeSystem.Interface;
using Wpf2Html5.TypeSystem.Items;

namespace Wpf2Html5.TypeSystem.Profiles
{
    /// <summary>
    /// Loads a component profile from an XML file.
    /// </summary>
    class ProfileLoader
    {
        private enum State { initial, source, profile, type };

        #region Properties
        public ITypeRoot Context { get; private set; }

        public string AssemblyName { get; private set; }

        public string ScriptAssemblyName { get; private set; }

        public string ScriptPath { get; private set; }

        public string Namespace { get; private set; }

        public string TypeName { get; private set; }

        public string BaseTypeName { get; private set; }

        public ITypeItem LType { get; private set; }

        #endregion

        #region Construction

        public ProfileLoader(ITypeRoot context)
        {
            Context = context;
        }

        #endregion

        #region Diagnostics

        private void Trace(string format, params object[] args)
        {
            if (Log.ShowProfileLoader)
            {
                Log.Trace(format, args);
            }
        }

        #endregion

        public void LoadResource(string p)
        {
            using (var s = typeof(ProfileLoader).Assembly.GetManifestResourceStream("Wpf2Html5.TypeSystem.Profiles." + p + ".xml"))
            {
                if (null == s)
                {
                    throw new Exception("resource profile '" + p + "' not found.");
                }

                using (var reader = XmlReader.Create(s))
                {
                    LoadProfile(reader);
                }
            }
        }

        public void LoadProfile(XmlReader reader)
        {
            reader.MoveToContent();

            State s = State.initial;

            while (reader.NodeType != XmlNodeType.None)
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    if (reader.LocalName == "profile")
                    {
                        if (s == State.initial)
                        {
                            s = State.profile;

                            ScriptAssemblyName = reader.GetAttribute("script-assembly");
                            ScriptPath = reader.GetAttribute("script-path");
                        }
                        else
                        {
                            throw new MalformedConfigurationException("unexpected 'profile' element.");
                        }
                    }
                    else if (reader.LocalName == "source")
                    {
                        if (s == State.profile)
                        {
                            Namespace = reader.GetAttribute("namespace");
                            AssemblyName = reader.GetAttribute("assembly") ?? "mscorlib";
                            BaseTypeName = reader.GetAttribute("base");

                            s = State.source;
                        }
                        else
                        {
                            throw new MalformedConfigurationException("unexpected 'source' element.");
                        }
                    }
                    else if (reader.LocalName == "type")
                    {
                        if (s == State.source)
                        {
                            TypeName = reader.GetAttribute("name");

                            var oldbase = BaseTypeName;
                            var z = reader.GetAttribute("base");
                            if (null != z)
                            {
                                BaseTypeName = z;
                            }

                            InstallType();

                            BaseTypeName = oldbase;

                            if (reader.IsStartElement() && !reader.IsEmptyElement)
                            {
                                s = State.type;
                            }
                            else
                            {
                                InstallTypeScript();
                                LType = null;
                            }
                        }
                        else
                        {
                            throw new MalformedConfigurationException("unexpected 'type' element.");
                        }
                    }
                    else if (reader.LocalName == "script")
                    {
                        if (s == State.type)
                        {
                            InstallScript(reader.GetAttribute("name"), true);
                        }
                        else if(s == State.source)
                        {
                            InstallScript(reader.GetAttribute("name"), true);
                        }
                        else
                        {
                            throw new MalformedConfigurationException("unexpected '" + reader.LocalName + "' element.");
                        }
                    }
                    else
                    {
                        throw new MalformedConfigurationException("unexpected '" + reader.LocalName + "' element.");
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (s == State.profile)
                    {
                        s = State.initial;
                    }
                    else if (s == State.source)
                    {
                        s = State.profile;
                    }
                    else if (s == State.type)
                    {
                        InstallTypeScript();
                        LType = null;
                        s = State.source;
                    }
                    else
                    {
                        throw new MalformedConfigurationException("unexpected end element.");
                    }
                }

                reader.Read();
            }
        }


        #region Private Methods

        private Assembly GetAssembly(string name)
        {
            var asm = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => a.GetName().Name == name)
                .FirstOrDefault();

            if (null == asm)
            {
                asm = Assembly.Load(name);
            }

            return asm;
        }

        private void InstallScript(string name, bool required)
        {
            if (null != ScriptAssemblyName)
            {
                var scriptpath = ScriptPath + "." + name + ".js";
                var asm = GetAssembly(ScriptAssemblyName);
                var path = asm.GetName().Name + "." + scriptpath;

                using(var s = asm.GetManifestResourceStream(path))
                {
                    if(null == s)
                    {
                        if(required)
                        {
                            throw new Exception("script '" + path + "' was not found in " + asm.GetName().Name + ".");
                        }
                    }
                    else if(null != LType)
                    {
                        Trace("  referencing script '" + path + "'.");
                        LType.AddScriptReference(asm, scriptpath);
                    }
                    else
                    {
                        Trace("referencing script '" + path + "'.");
                        Context.AddScriptReference(asm, scriptpath);
                    }
                }
            }
        }

        private Type Lookup(string name)
        {
            var asm = GetAssembly(AssemblyName);
            var fullname = name;
            if (!name.Contains('.'))
            {
                fullname = Namespace + "." + name;
            }

            var rtype = asm.GetType(fullname);

            if (null == rtype)
            {
                throw new UnresolvedTypeException("type '" + fullname + "' could not be resolved in " + AssemblyName + ".");
            }

            return rtype;
        }

        private void InstallType()
        {
            var rtype = Lookup(TypeName);

            Trace("constructing {0} ...", rtype.FullName);

            Type rbase = null;
            if(null != BaseTypeName)
            {
                rbase = Lookup(BaseTypeName);
            }

            if(null != rbase && rbase.FullName == rtype.FullName)
            {
                rbase = null;
            }

            // registers itself
            LType = new NativeTypeRef(Context, rtype, true, rbase);
        }

        private void InstallTypeScript()
        {
            var rawname = LType.RType.Name.Split('`').First();
            InstallScript(rawname, false);
        }

        #endregion
    }
}
