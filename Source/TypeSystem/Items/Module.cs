using System;
using System.Collections.Generic;
using System.Reflection;
using Wpf2Html5.TypeSystem.Interface;

namespace Wpf2Html5.TypeSystem.Items
{
    /// <summary>
    /// Represents an individual module.
    /// </summary>
    class Module : Container, IModule
    {
        private List<string> _namespaceinorder = new List<string>();
        private List<Assembly> _assemblies = new List<Assembly>();
        private Dictionary<string, List<Assembly>> _namespaceassemblies = new Dictionary<string, List<Assembly>>();

        #region Construction

        public Module(ITypeRoot root, string name)
            : base(root, name)
        {
        }

        #endregion

        #region Overrides

        protected override ITypeItem BeforeTranslateRuntimeType(Type rtype)
        {
            if (rtype.IsGenericType && !rtype.ContainsGenericParameters)
            {
                // get the base generic type from the lower layer.
                var generictype = ParentTypes.TranslateRType(rtype, TranslateOptions.None);

                // and create a generic type object ontop
                var generic = new GenericTypeRef(this, generictype, rtype);

                // which we add at this level
                Types.AddItem(generic);

                return generic;
            }
            else
            {
                return base.BeforeTranslateRuntimeType(rtype);
            }
        }

        #endregion

        public void AddSourceNamespace(string ns, Assembly assembly)
        {
            if (null == ns) throw new ArgumentNullException("ns");
            if (null == assembly) throw new ArgumentNullException("assembly");

            // add to list ordered by insertion sequence.
            if (!_namespaceinorder.Contains(ns))
            {
                _namespaceinorder.Add(ns);
            }

            if (!_assemblies.Contains(assembly))
            {
                _assemblies.Add(assembly);
            }

            // add associated assemblies
            List<Assembly> list;
            if (!_namespaceassemblies.TryGetValue(ns, out list))
            {
                _namespaceassemblies[ns] = list = new List<Assembly>();
            }

            if (!list.Contains(assembly))
            {
                list.Add(assembly);
            }

            Trace("namespace [{0}] -> {1}.", ns, assembly.GetName().Name);
        }

        public override Assembly[] Assemblies
        {
            get { return _assemblies.ToArray(); }
        }

        protected override bool IsGeneratedRType(Type type)
        {
            return (_namespaceassemblies.ContainsKey(type.Namespace)
                || (null != ShouldTranslateType && ShouldTranslateType(type)))
                && null == type.GetCustomAttribute(typeof(GeneratorIgnoreAttribute), false);
        }

    }
}
