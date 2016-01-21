using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf2Html5.TypeSystem.Interface;

namespace Wpf2Html5.TypeSystem.Comparers
{
    class TypeItemEqualityComparer : IEqualityComparer<ITypeItem>
    {
        public bool Equals(ITypeItem x, ITypeItem y)
        {
            return x.ID == y.ID;
        }

        public int GetHashCode(ITypeItem obj)
        {
            return obj.ID.GetHashCode();
        }
    }
}
