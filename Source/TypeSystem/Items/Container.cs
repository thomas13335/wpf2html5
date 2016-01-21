using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Wpf2Html5.Exceptions;
using Wpf2Html5.TypeSystem.Collections;
using Wpf2Html5.TypeSystem.Interface;

namespace Wpf2Html5.TypeSystem.Items
{
    /// <summary>
    /// Base class for declaration containers (Module, Class, ...).
    /// </summary>
    abstract class Container : ItemBase, IDeclarationContext, IDebugContext, IAssemblyContainer
    {
        #region Private

        private VariableCollection _variables;
        private ItemsCollection _types;

        private static Regex _extractgeneric = new Regex(@"\<[^\>]+\>");

        #endregion

        #region Public Properties

        internal ItemsCollection Types { get { return _types; } }

        public Func<Type, bool> ShouldTranslateType { get; set; }

        public override IEnumerable<ITypeItem> Members
        {
            get { return _types; }
        }

        public IVariableContext LocalVariables { get { return _variables; } }


        #endregion

        #region Construction

        public Container(IDeclarationContext parent, string name)
            : base(parent, name)
        {
            _variables = new VariableCollection(this);
            _types = new ItemsCollection();
        }

        public Container(IDeclarationContext parent, Type rtype)
            : this(parent, rtype.GetRName())
        { }

        #endregion

        #region Diagnostics

        public override string ToDebugString(int depth = 0, bool expand = false)
        {
            var prefix = new string(' ', 2 * depth);
            var sb = new StringBuilder();

            sb.Append(_variables.ToDebugString(depth + 1));
            sb.Append(_types.ToDebugString(depth + 1, expand));

            return sb.ToString();
        }

        #endregion

        #region ITypeContext

        public virtual ITypeItem TranslateRType(Type rtype, TranslateOptions options)
        {
            ITypeItem result;

            ValidateRType(rtype);

            // see if qualified type name is already in the list
            var qname = rtype.GetRName();
            if (!Types.TryGetItem(qname, out result))
            {
                // let subclass do its task
                result = BeforeTranslateRuntimeType(rtype);

                if (null == result)
                {
                    // default flow, let parent do ...
                    if (null != ParentTypes)
                    {
                        result = ParentTypes.TranslateRType(rtype, TranslateOptions.None);
                    }

                    if (null == result)
                    {
                        // type does not exist, add it
                        result = AddRuntimeType(rtype);
                    }
                }
            }

            if (null == result && options.IsMustExist())
            {
                throw new UnresolvedTypeException("runtime type '" + qname + "' could not be resolved.");
            }

            // Log.Trace("[{0}] TranslateRType({1}, {2}) => {3}", this, rtype.Name, options, result);

            return result;
        }

        private void Debug(bool p)
        {
            throw new NotImplementedException();
        }

        protected virtual ITypeItem BeforeTranslateRuntimeType(Type rtype)
        {
            return null;
        }

        protected virtual bool IsGeneratedRType(Type rtype)
        {
            return false;
        }

        protected virtual ITypeItem AddRuntimeType(Type type)
        {
            ITypeItem result;

            if (type.IsGenericType)
            {
                // pass generic types to Module level
                result = Parent.TranslateRType(type, true);
            }
            else if (type.IsEnum)
            {
                result = new Enumeration(GetTypeDeclarationContext(), type);
            }
            else
            {
                // convert this type (JL3A77746Z)?
                if (IsGeneratedRType(type))
                {
                    result = new Class(GetTypeDeclarationContext(), type);
                }
                else
                {
                    // explicitly external class
                    result = new NativeTypeRef(GetTypeDeclarationContext(), type);
                    result.SetExternal();
                }
            }

            return result;
        }

        private void ValidateRType(Type type)
        {
            if (type.IsGenericParameter)
            {
                // TODO: generic types are not supported in some cases.
                if (null != type.DeclaringMethod)
                {
                    throw new Exception("unexpected generic parameter in method " + type.DeclaringMethod.Name + " of " +
                        type.DeclaringType.FullName);
                }
                else
                {
                    throw new Exception("unexpected generic parameter.");
                }
            }
        }

        public virtual ITypeItem ResolveLType(string name)
        {
            ITypeItem result;

            result = _types.GetItem(name);

            if (null == result)
            {
                // check parent context
                if (null != ParentTypes)
                {
                    result = ParentTypes.ResolveLType(name);
                }
            }

            return result;
        }

        /// <summary>
        /// Returns the type context to be used for registration of new items.
        /// </summary>
        /// <returns>This context by default.</returns>
        public override IDeclarationContext GetTypeDeclarationContext()
        {
            return this;
        }

        public void RegisterType(ITypeItem ltype)
        {
            Trace("[RegisterType] <{0}> {1}", this.ID, ltype);

            Types.AddItem(ltype);
        }

        public virtual void AddDepdendency(ITypeItem dependent, ITypeItem target)
        {
            if (null != ParentTypes)
            {
                ParentTypes.AddDepdendency(dependent, target);
            }
        }

        #endregion

        #region IVariableContext

        public virtual ITypeItem GetVariable(string name)
        {
            var result = LocalVariables.GetVariable(name);
            if (null == result && null != Parent)
            {
                result = Parent.GetVariable(name);
            }

            return result;
        }

        public void AddVariable(string name, ITypeItem type)
        {
            _variables.AddVariable(name, type);
        }

        public void AddVariable(ITypeItem item)
        {
            _variables.AddItem(item);
        }

        #endregion

        #region IAssemblyContainer

        public virtual Assembly[] Assemblies
        {
            get { return new Assembly[0]; }
        }


        #endregion

        #region Public Methods

        #endregion

        #region Protected Methods

        protected ITypeItem GetLocalVariable(string name)
        {
            return _variables.GetVariable(name);
        }

        protected void AddTypeItem(ITypeItem item)
        {
            _types.AddItem(item);
        }

        /*
        protected ITypeItem ResolveLTypeFromNamespaces(string typename)
        {
            foreach (var ns in _namespaceinorder)
            {
                foreach (var assembly in _namespaceassemblies[ns])
                {
                    var fullname = ns + "." + typename;
                    var type = assembly.GetType(fullname);
                    if (null != type)
                    {
                        var ltype = GetLType(type);
                        Trace("typename '{0}' resolved to [{1}].", typename, ltype);
                        return ltype;
                    }
                }
            }

            Log.Warning("typename '{0}' not resolved in any namespace '{1}'.", typename, _namespaceinorder.ToSeparatorList());
            return null;
        }*/

        #endregion
    }
}
