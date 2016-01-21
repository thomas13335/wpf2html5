using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf2Html5.TypeSystem.Interface;

namespace Wpf2Html5.TypeSystem.Items
{
    /// <summary>
    /// Represents a class method.
    /// </summary>
    class Method : ItemWithType
    {
        public Method(IVariableContext parent, string id, ITypeItem ltype)
            : base(parent, id, ltype)
        {
        }

        public Method(IDeclarationContext parent, string id, ITypeItem ltype)
            : base(parent, id, ltype)
        {
        }
    }
}
