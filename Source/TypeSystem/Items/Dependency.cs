using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf2Html5.TypeSystem.Interface;

namespace Wpf2Html5.TypeSystem.Items
{
    class Dependency : IDependency
    {
        public ITypeItem Target { get; private set; }

        public DependencyLevel Level { get; private set; }

        public Dependency(ITypeItem target, DependencyLevel level)
        {
            Target = target;
            Level = level;
        }
    }
}
