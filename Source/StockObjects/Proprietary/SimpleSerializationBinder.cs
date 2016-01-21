using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;

namespace Wpf2Html5.StockObjects
{
#if JSON_SERIALIZATION_BINDER
    /// <summary>A serialization binding for the JSON '$type' field.</summary>
    [GeneratorIgnore]
    public class SimpleSerializationBinder : Newtonsoft.Json.SerializationBinder
    {
        #region Private

        class BinderNamespace
        {
            public string Namespace { get; private set; }

            public Assembly Assembly { get; private set; }

            public BinderNamespace(string ns, Assembly assembly)
            {
                Namespace = ns;
                Assembly = assembly;
            }
        }

        private List<BinderNamespace> _namespaces = new List<BinderNamespace>();

        #endregion

        public SimpleSerializationBinder()
        {
            var type = GetType();
        }

        /// <summary>Adds a namespace assembly mapping to the list of available namespaces.</summary>
        /// <param name="ns">The CLR namespace.</param>
        /// <param name="assembly">The assembly to register.</param>
        public void Add(string ns, Assembly assembly)
        {
            _namespaces.Add(new BinderNamespace(ns, assembly));
        }

        #region Serialization Binder Methods

        public override Type BindToType(string assemblyName, string typeName)
        {
            Type result = null;
            foreach(var ns in _namespaces)
            {
                var fullname = ns.Namespace + "." + typeName;
                result = ns.Assembly.GetType(fullname);
                if (null != result) break;
            }

            if(null == result)
            {
                throw new Exception("unable to resolve type '" + typeName + "'.");
            }

            return result;
        }

        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            assemblyName = null;
            typeName = serializedType.Name;
        }

        #endregion
    }

#endif
}
