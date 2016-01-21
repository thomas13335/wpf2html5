using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wpf2Html5.TypeSystem.Interface;

namespace Wpf2Html5.TypeSystem.Items
{
    /// <summary>
    /// Represents a class declaration to be converted.
    /// </summary>
    class Class : Container, IClassContext
    {
        #region Private Fields

        private Type _rtype;
        private IVariableContext _lbase;
        private Variable _thisvar;
        private ITypeItem _baseclass;
        private bool _preparing;

        #endregion

        #region Properties

        public override bool IsClass { get { return true; } }

        public bool IsEnum { get { return _rtype.IsEnum; } }

        public bool IsConstructed { get { return !_rtype.IsEnum && !_rtype.IsValueType; } }

        public override Type RType { get { return _rtype; } }

        public override ITypeItem BaseType { get { return _baseclass; } }

        #endregion

        #region Construction

        public Class(IDeclarationContext parent, Type type)
            : base(parent, type)
        {
            // save the underlying runtime type
            _rtype = type;
            _thisvar = new Variable(this, "this", this);

            Parent.RegisterType(this);

            ExtractBaseType();
            ExtractNestedTypes();

            if (IsConstructed)
            {
                ExtractMembers();
            }
        }

        #endregion

        #region Public Methods

        public override ITypeItem GetVariable(string name)
        {
            ITypeItem result;

            if (name == "this")
            {
                result = _thisvar;
            }
            else if (name == "base")
            {
                result = _baseclass;
            }
            else if (name == "trace")
            {
                // TODO: special case?
                result = null == Parent ? null : Parent.GetVariable(name);
            }
            else
            {
                // try local variable
                result = LocalVariables.GetVariable(name);
                if (null == result && null != _lbase)
                {
                    // look at base class
                    result = _lbase.GetVariable(name);
                    if (null == result && null != Parent)
                    {
                        // parent?
                        result = Parent.GetVariable(name);
                    }
                }
            }

            return result;
        }

        public override ITypeItem ResolveLType(string name)
        {
            ITypeItem result;

            if (name == ID)
            {
                // the name corresponds to the name of this class.
                result = this;
            }
            else
            {
                // look in the types collection
                result = Types.GetItem(name);

                if (null == result)
                {
                    // divert to base class if applicable
                    if (_baseclass is ITypeContext)
                    {
                        // let baseclass do the rest
                        result = ((ITypeContext)_baseclass).ResolveLType(name);
                    }
                    else
                    {
                        // toplevel class
                        result = base.ResolveLType(name);
                    }
                }
            }

            return result;
        }

        public override IDeclarationContext GetTypeDeclarationContext()
        {
            return Parent.GetTypeDeclarationContext();
        }

        public override bool Prepare()
        {
            var result = true;

            try
            {
                if (_preparing)
                {
                    // don't recurse into cycles
                    result = true;
                }
                else
                {
                    _preparing = true;
                    if (GStatus == TypeGenerationStatus.source)
                    {
                        GStatus = TypeGenerationStatus.preparing;
                        foreach (var d in Dependencies
                            .Select(g => g.Target)
                            .Where(g => g.GStatus == TypeGenerationStatus.source))
                        {
                            if (!d.Prepare())
                            {
                                Log.Trace("class {0} failed to prepare ({1}).", d, d.GStatus);
                                result = false;
                            }
                        }
                    }
                    else if(GStatus == TypeGenerationStatus.preparing)
                    {

                    }
                    else if (GStatus != TypeGenerationStatus.convert)
                    {
                        result = false;
                    }

                    if (result)
                    {
                        if (RType.IsInterface)
                        {
                            // don't need any representation of an interface.
                            GStatus = TypeGenerationStatus.cloaked;
                        }
                        else
                        {
                            GStatus = TypeGenerationStatus.convert;
                        }
                    }
                    else if (GStatus != TypeGenerationStatus.failed)
                    {
                        Log.Trace("class {0} failed.", ID);
                        GStatus = TypeGenerationStatus.failed;
                    }
                }
            }
            finally
            {
                _preparing = false;
            }

            return result;
        }

        #endregion

        #region Private Methods

        private void ExtractBaseType()
        {
            var type = _rtype;
            var basetype = type.BaseType;

            if (null != basetype)
            {
                if (basetype == typeof(object) || basetype.IsValueType)
                {
                    basetype = null;
                }
            }

            if (null != basetype)
            {
                _baseclass = ParentTypes.TranslateRType(basetype, TranslateOptions.Add);
                _lbase = _baseclass as IVariableContext;
                if (null == _lbase)
                {
                    throw new Exception("base type [" + basetype.GetRName() + "] is not defined.");
                }

                AddDepdendency(_baseclass, DependencyLevel.Constructor);
            }
        }

        private void ExtractNestedTypes()
        {
            foreach (var typeinfo in _rtype
                .GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic)
                .Where(m => m.DeclaringType == _rtype))
            {
                if (null != typeinfo.GetCustomAttribute(typeof(GeneratorIgnoreAttribute)))
                {
                    continue;
                }

                ITypeItem ltype = null;
                if (typeinfo.IsValueType)
                {
                    if (typeinfo.IsEnum)
                    {
                        ltype = TranslateRType(typeinfo, TranslateOptions.Add);
                    }
                    else
                    {
                        // ignore value type
                    }
                }
                else
                {
                    ltype = TranslateRType(typeinfo, TranslateOptions.Add);
                }

                // add the type name at this class level also
                Types.AddItem(typeinfo.Name, ltype);
            }
        }

        private void ExtractMembers()
        {
            var flags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            foreach (var memberinfo in _rtype
                .GetMembers(flags)
                .Where(m => m.DeclaringType == _rtype))
            {
                string name = memberinfo.Name;
                ITypeItem ltype;
                // TraceTarget.Trace("{0,-20} {1}", name, memberinfo.GetType().Name);

                if (memberinfo is FieldInfo)
                {
                    var fieldinfo = memberinfo as FieldInfo;
                    var rtype = fieldinfo.FieldType;
                    ltype = MapType(rtype);
                    AddVariable(name, ltype);
                }
                else if (memberinfo is MethodInfo)
                {
                    var methodinfo = (MethodInfo)memberinfo;
                    ltype = MapType(methodinfo.ReturnType);
                    var method = new Method(this, methodinfo.Name, ltype);
                    AddVariable(method);

                }
                else if (memberinfo is PropertyInfo)
                {
                    var propinfo = memberinfo as PropertyInfo;
                    var rtype = propinfo.PropertyType;
                    ltype=MapType(rtype);
                    AddVariable(new Property(this, name, ltype));
                }
                else if (memberinfo is ConstructorInfo)
                {
                    // constructors ignored.
                    continue;
                }
                else if (memberinfo is Type)
                {
                    // nested type, done elsewhere
                    continue;
                }
                else
                {
                    Log.Warning("member '{0}' could not be extracted [{1}].", name, memberinfo);
                    continue;
                }

                // get the newly created variable
                var item = GetVariable(name);

                if(null == item)
                {
                    throw new Exception("variable '" + name + "' not resolved in " + this + ".");
                }

                if(null != ltype)
                {
                    AddDepdendency(ltype, DependencyLevel.Lowest);
                }

                /*if (null != item.LType)
                {
                    AddDepdendency(this, item.LType);
                }*/


                if (null != memberinfo.GetCustomAttribute(typeof(GeneratorIgnoreAttribute)))
                {
                    item.SetDoNotGenerate();
                }
            }
        }

        private void ExtractMethod(MethodInfo methodinfo)
        {
        }

        #endregion
    }
}
