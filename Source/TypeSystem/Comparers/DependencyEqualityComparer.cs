using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf2Html5.TypeSystem.Interface;

namespace Wpf2Html5.TypeSystem.Comparers
{
    class DependencyEqualityComparer : IEqualityComparer<IDependency>
    {
        public bool Equals(IDependency x, IDependency y)
        {
            if(x.Level == y.Level)
            {
                if(x.Target.ID == y.Target.ID)
                {
                    return true;
                }
            }

            return false;
        }

        public int GetHashCode(IDependency obj)
        {
            return (obj.Target + "|" + obj.Level).GetHashCode();
        }
    }
}
