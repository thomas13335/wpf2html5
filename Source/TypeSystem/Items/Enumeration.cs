using System;
using System.Linq;
using Wpf2Html5.TypeSystem.Interface;

namespace Wpf2Html5.TypeSystem.Items
{
    /// <summary>
    /// Represents an enumeration type.
    /// </summary>
    class Enumeration : ItemBase
    {
        public override bool IsEnumeration { get { return true; } }

        public Enumeration(IDeclarationContext parent, Type enumtype)
            : base(parent, enumtype)
        {
            parent.RegisterType(this);
        }

        public override ITypeItem GetMember(string name)
        {
            var index = RType.GetEnumNames().ToList().IndexOf(name);
            if (index < 0)
            {
                // not an enumeration item, look for others ...
                var basetype = ParentTypes.TranslateRType(typeof(object), false);
                return basetype.GetMember(name);
            }
            else
            {
                return new Constant(Parent, this, name);
            }
        }
    }
}
