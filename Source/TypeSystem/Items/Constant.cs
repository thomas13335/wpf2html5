using Wpf2Html5.TypeSystem.Interface;

namespace Wpf2Html5.TypeSystem.Items
{
    class Constant : ItemBase
    {
        private ITypeItem _ltype;

        public override ITypeItem LType
        {
            get { return _ltype; }
        }

        public Constant(IDeclarationContext parent, ITypeItem type, object value)
            : base(parent, value.ToString())
        {
            _ltype = type;
        }
    }
}
