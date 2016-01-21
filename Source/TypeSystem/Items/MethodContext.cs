using System;
using Wpf2Html5.TypeSystem.Collections;
using Wpf2Html5.TypeSystem.Interface;

namespace Wpf2Html5.TypeSystem.Items
{
    /// <summary>
    /// Declaration context used during method translation.
    /// </summary>
    class MethodContext : IMethodContext
    {
        #region Private

        private IDeclarationContext _parent;
        private VariableCollection _variables;

        #endregion

        public IDeclarationContext Parent { get { return _parent; } }

        #region Construction

        public MethodContext()
        {
            _variables = new VariableCollection(this);
        }

        public MethodContext(IDeclarationContext parent)
            : this()
        {
            Runtime.Assert(null != parent);
            _parent = parent;
        }

        #endregion

        public IDeclarationContext GetTypeDeclarationContext()
        {
            return _parent.GetTypeDeclarationContext();
        }

        public ITypeItem GetVariable(string name)
        {
            ITypeItem result;
            if(null == (result = _variables.GetVariable(name)))
            {
                result = _parent.GetVariable(name);
            }
            else
            {
            }

            return result;
        }

        public void AddVariable(string name, ITypeItem type)
        {
            _variables.AddVariable(name, type);
        }

        #region ITypeContext

        public ITypeItem ResolveLType(string name)
        {
            return _parent.ResolveLType(name);
        }

        public ITypeItem TranslateRType(Type rtype, TranslateOptions options)
        {
            return Parent.TranslateRType(rtype, options);
        }

        public void RegisterType(ITypeItem ltype)
        {
            throw new NotImplementedException("unable to register a type within a method context.");
        }

        #endregion

        public string ToDebugString(int depth = 0, bool expand = false)
        {
            return _variables.ToDebugString(depth, expand);
        }

    }
}
