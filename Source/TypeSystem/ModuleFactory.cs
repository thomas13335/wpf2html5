using System;
using Wpf2Html5.TypeSystem;
using Wpf2Html5.TypeSystem.Interface;
using Wpf2Html5.TypeSystem.Items;

namespace Wpf2Html5.TypeSystem
{
    /// <summary>
    /// Creates modules for code translation and holds configuration properties.
    /// </summary>
    public static class ModuleFactory
    {
        private static Root _instance;

        /// <summary>
        /// Returns the static type system root.
        /// </summary>
        public static ITypeRoot System { get { return GetInstance(); } }


        /// <summary>
        /// Creates a new application module.
        /// </summary>
        /// <param name="name">The name of the module.</param>
        /// <returns>The interface to the new module.</returns>
        public static IModule CreateModule(string name)
        {
            return new Module(System, name);
        }

        /// <summary>
        /// Creates a method context.
        /// </summary>
        /// <param name="dc">The declaration context on which to base the method context, must be a class.</param>
        /// <returns>The method context.</returns>
        public static IMethodContext CreateMethodContext(IDeclarationContext dc)
        {
            return new MethodContext(dc);
        }

        private static ITypeRoot GetInstance()
        {
            if (null == _instance)
            {
                _instance = new Root();
            }

            return _instance;
        }
    }
}
