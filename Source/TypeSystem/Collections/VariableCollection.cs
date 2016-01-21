using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf2Html5.TypeSystem.Interface;
using Wpf2Html5.TypeSystem.Items;

namespace Wpf2Html5.TypeSystem.Collections
{
    class VariableCollection : ItemsCollection, IVariableContext
    {
        private IDeclarationContext _parent;

        public ITypeItem GetVariable(string id)
        {
            return GetItem(id);
        }

        public VariableCollection(IDeclarationContext parent)
        {
            _parent = parent;
        }

        public void AddVariable(string id, ITypeItem type)
        {
            IVariableContext context = _parent ?? (this as IVariableContext);

            AddItem(new Variable(context, id, type));
        }


        public IDeclarationContext GetTypeDeclarationContext()
        {
            return _parent;
        }
    }
}
