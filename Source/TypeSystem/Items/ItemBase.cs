using System;
using System.Collections.Generic;
using System.Reflection;
using Wpf2Html5.TypeSystem;
using Wpf2Html5.TypeSystem.Interface;

namespace Wpf2Html5.TypeSystem.Items
{
    abstract class ItemBase : ITypeItem
    {
        protected string _id;
        private Type _rtype;
        protected IDeclarationContext _parent;
        private object _astitem;

        #region Properties

        /// <summary>
        /// unique identifier for this item within a context.
        /// </summary>
        public string ID { get { return _id; } }

        public IDeclarationContext Parent { get { return _parent; } }

        public virtual bool IsClass { get { return false; } }

        public virtual object SourceNode { get { return _astitem; } }

        public virtual string CodeName
        {
            get
            {
                if (null == RType)
                {
                    throw new InvalidOperationException("runtime type is not specified for " + this);
                }

                return RType.GetCodeName();
            }
        }

        public virtual Type RType { get { return _rtype; } }

        public virtual ITypeItem LType { get { return null; } }

        public virtual ITypeItem BaseType { get { return null; } }

        public ITypeContext ParentTypes { get { return _parent as ITypeContext; } }

        public virtual bool IsMember { get { return Parent is IClassContext; } }

        public virtual bool IsProperty { get { return false; } }

        public virtual bool IsBase { get { return false; } }

        public virtual bool IsDelegate { get { return false; } }

        public virtual bool IsEnumeration { get { return false; } }

        public virtual bool DoNotGenerate
        {
            get
            {
                return GStatus == TypeGenerationStatus.cloaked
                    || GStatus == TypeGenerationStatus.external;
            }
        }

        public virtual IEnumerable<ITypeItem> Dependencies { get { return new ITypeItem[0]; } }

        public virtual IEnumerable<ITypeItem> Members { get { yield break; } }

        public TypeGenerationStatus GStatus { get; protected set; }

        public virtual IEnumerable<IScriptReference> ScriptReferences { get { return new IScriptReference[0]; } }

        #endregion

        #region Construction

        private ItemBase(IDeclarationContext parent)
        {
            _parent = parent;
        }

        protected ItemBase(IDeclarationContext parent, string name)
            : this(parent)
        {
            _id = name;
        }

        protected ItemBase(IDeclarationContext parent, Type rtype)
            : this(parent)
        {
            _rtype = rtype;
            _id = rtype.GetRName();
        }

        #endregion

        #region Diagnostics

        public override string ToString()
        {
            return GetType().Name + "(" + ID + ")";
        }

        public virtual string ToDebugString(int depth = 0, bool expand = false)
        {
            return ToString();
        }

        public bool Verbose { get; set; }

        protected void Trace(string format, params object[] args)
        {
            if (Log.ShowTypeSystem)
            {
                var prefix = "[" + GetType().Name + ":" + ID + "] ";
                Log.Trace(prefix + format, args);
            }
        }

        protected void Information(string format, params object[] args)
        {
            Log.Trace(format, args);
        }

        #endregion

        protected ITypeItem MapType(Type rtype)
        {
            return GetTypeDeclarationContext().TranslateRType(rtype, TranslateOptions.MustExist);
        }

        public void SetDoNotGenerate()
        {
            //_donotgenerate = true;
            if (GStatus == TypeGenerationStatus.initial)
            {
                GStatus = TypeGenerationStatus.cloaked;
            }
            else if (GStatus == TypeGenerationStatus.cloaked)
            {
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public virtual IDeclarationContext GetTypeDeclarationContext()
        {
            return Parent;
        }

        public virtual void SetDisableTranslation(params string[] members)
        { }

        public virtual void SetSourceNode(object astitem)
        {
            _astitem = astitem;

            if (GStatus == TypeGenerationStatus.initial)
            {
                GStatus = TypeGenerationStatus.source;
            }
        }

        public virtual void SetExternal()
        {
            if (GStatus == TypeGenerationStatus.initial)
            {
                GStatus = TypeGenerationStatus.external;
            }
        }

        public virtual bool Prepare()
        {
            return true;
        }

        public virtual void SetFailed()
        {
            GStatus = TypeGenerationStatus.failed;
        }

        public virtual ITypeItem GetMember(string name)
        {
            var variables = this as IVariableContext;
            if (null != variables)
            {
                return variables.GetVariable(name);
            }
            else
            {
                return null;
            }
        }

        public virtual ITypeItem GetConvertMethod()
        {
            return null;
        }

        public virtual void AddScriptReference(Assembly assembly, string path)
        {
            throw new NotImplementedException();
        }
    }
}
