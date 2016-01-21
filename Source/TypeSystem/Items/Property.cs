
using Wpf2Html5.TypeSystem.Interface;

namespace Wpf2Html5.TypeSystem.Items
{
    /// <summary>
    /// Represents a property of a class.
    /// </summary>
    class Property : ItemWithType
    {
        public override bool IsProperty { get { return true; } }

        public Property(IVariableContext parent, string id, ITypeItem type)
            : base(parent, id, type)
        {
        }
    }
}
