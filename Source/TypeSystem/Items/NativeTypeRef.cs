using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Wpf2Html5.TypeSystem.Interface;

namespace Wpf2Html5.TypeSystem.Items
{
    /// <summary>
    /// Represents a data type that is implemented by specific JS code.
    /// </summary>
    class NativeTypeRef : ItemBase, IVariableContext, IClassContext
    {
        #region Private

        private SortedSet<string> _untranslatedmembers = new SortedSet<string>();
        private List<ScriptReference> _scriptrefs = new List<ScriptReference>();
        private bool _isdelegate;

        #endregion

        #region Properties

        public override bool IsClass { get { return null == RType ? true : RType.IsClass; } }

        public override bool IsDelegate { get { return _isdelegate; } }

        public override string CodeName { get { return RType.GetCodeName(); } }

        public override IEnumerable<IScriptReference> ScriptReferences { get { return _scriptrefs; } }

        public override bool DoNotGenerate
        {
            get { return true; }
        }

        #endregion

        #region Construction

        public NativeTypeRef(IDeclarationContext parent, Type type, bool external = false)
            : base(parent, type)
        {
            if(null == type)
            {
                throw new ArgumentNullException("type");
            }

            if(external)
            {
                SetExternal();
            }

            var basetype = type;
            while (null != basetype)
            {
                if (basetype.FullName == "System.Delegate")
                {
                    _isdelegate = true;
                    break;
                }

                basetype = basetype.BaseType;
            }

            parent.RegisterType(this);
        }

        public NativeTypeRef(IDeclarationContext parent, string typename)
            : base(parent, typename)
        {
            SetExternal();
            parent.RegisterType(this);
        }

        #endregion

        #region Diagnostics

        public string ToDebugString() { return ToString(); }

        #endregion

        #region Public Methods

        public override ITypeItem GetMember(string name)
        {
            return GetVariable(name);
        }

        public ITypeItem GetVariable(string name)
        {
            if (_untranslatedmembers.Contains(name))
            {
                return new Variable(this, name, ParentTypes.GetLType(typeof(object)));
            }

            var memberinfo = GetMemberRecurse(RType, name);

            if (memberinfo.MemberType == MemberTypes.Method)
            {
                var methodinfo = (MethodInfo)memberinfo;
                var rettype = methodinfo.ReturnType;
                var ltype = ParentTypes.TranslateRType(rettype, TranslateOptions.MustExist);
                return new Method(this, name, ltype);
            }
            else if (memberinfo.MemberType == MemberTypes.Property)
            {
                var propertyinfo = (PropertyInfo)memberinfo;
                var ltype = ParentTypes.TranslateRType(propertyinfo.PropertyType, TranslateOptions.MustExist);
                return new Property(this, name, ltype);
            }
            else if (memberinfo.MemberType == MemberTypes.Field)
            {
                var fieldinfo = (FieldInfo)memberinfo;
                var ltype = ParentTypes.TranslateRType(fieldinfo.FieldType, TranslateOptions.MustExist);
                return new Field(this, name, ltype);
            }
            else
            {
                throw new NotImplementedException("type item [" + memberinfo.MemberType + "] cannot be translated.");
            }
        }

        public void AddVariable(string name, ITypeItem type)
        {
            throw new NotImplementedException();
        }

        public override void AddScriptReference(Assembly assembly, string path)
        {
            _scriptrefs.Add(new ScriptReference(assembly, path));
        }

        public override ITypeItem GetConvertMethod()
        {
            if (RType.IsValueType)
            {
                return new Method(this, "Coerce" + CodeName, this);
            }
            else
            {
                return null;
            }
        }

        public override void SetDisableTranslation(params string[] members)
        {
            foreach (var member in members)
            {
                _untranslatedmembers.Add(member);
            }
        }

        public override bool Prepare()
        {
            return GStatus == TypeGenerationStatus.external;
        }

        #endregion

        #region Private Methods

        private MemberInfo GetMemberRecurse(Type type, string name)
        {
            MemberInfo result = null;

            // if(GetVariable(name))

            var memberinfos = type.GetMember(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (memberinfos.Any())
            {
                if (memberinfos.Count() > 1)
                {
                    // TODO: handle this case correctly? affects DependencyObject.SetValue e.g.
                    // Log.Warning("ambigous name {0} in {1}", name, this);
                }

                result = memberinfos.First();
            }
            else
            {
                // look at interfaces implemented
                foreach (var itf in type.GetInterfaces())
                {
                    if (null != (result = GetMemberRecurse(itf, name)))
                    {
                        break;
                    }
                }

                if (null == result && null != type.BaseType)
                {
                    result = GetMemberRecurse(type.BaseType, name);
                }
            }

            return result;
        }

        #endregion

    }
}
