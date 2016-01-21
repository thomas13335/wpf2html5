using System;
using Wpf2Html5.TypeSystem.Interface;

namespace Wpf2Html5.TypeSystem.Items
{
    /// <summary>
    /// Baseclass for a simple, typed object, such as a variable, method, property or field.
    /// </summary>
    abstract class ItemWithType : ItemBase
    {
        #region Private

        private ITypeItem _type;
        private object _container;

        #endregion

        #region Properties

        public override ITypeItem LType { get { return _type; } }

        public override Type RType { get { return LType.RType; } }

        public override string CodeName { get { return ID; } }

        public override bool IsMember { get { return _container is IClassContext; } }

        #endregion

        #region Construction

        /// <summary>
        /// Creates an types item based on a variable context.
        /// </summary>
        /// <param name="parent">The variable context.</param>
        /// <param name="id">The identifier of the item.</param>
        /// <param name="type">The type of the item.</param>
        public ItemWithType(IVariableContext parent, string id, ITypeItem type)
            : base(parent.GetTypeDeclarationContext(), id)
        {
            _container = parent;
            _type = type;
        }

        public ItemWithType(IDeclarationContext parent, string id, ITypeItem type)
            : base(parent, id)
        {
            _container = parent;
            _type = type;
        }

        #endregion
    }
}
