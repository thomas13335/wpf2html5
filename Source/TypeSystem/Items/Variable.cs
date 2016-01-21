using Wpf2Html5.TypeSystem.Interface;

namespace Wpf2Html5.TypeSystem.Items
{
    class Variable : ItemWithType
    {

        public Variable(IVariableContext parent, string name, ITypeItem type)
            : base(parent, name, type)
        {
        }
    }
}
