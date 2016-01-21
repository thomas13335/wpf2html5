using Wpf2Html5.TypeSystem.Interface;

namespace Wpf2Html5.TypeSystem.Items
{
    class Field : ItemWithType
    {
        public override bool IsProperty { get { return true; } }

        public Field(IVariableContext parent, string id, ITypeItem type)
            : base(parent, id, type)
        {
        }
    }
}
