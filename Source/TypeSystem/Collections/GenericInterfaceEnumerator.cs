using System;
using System.Collections.Generic;

namespace Wpf2Html5.TypeSystem.Collections
{
    /// <summary>
    /// Iterates the genericalized interfaces of a type.
    /// </summary>
    class GenericInterfaceEnumerator
    {
        private Type _type;

        public GenericInterfaceEnumerator(Type type)
        {
            _type = type;
        }

        public IEnumerable<Type> Interfaces
        {
            get
            {
                // also itself
                yield return _type.GetGenericTypeDefinition();

                foreach (var itf in _type.GetInterfaces())
                {
                    yield return itf.GetGenericTypeDefinition();
                }
            }
        }
    }
}
