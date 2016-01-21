using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wpf2Html5.TypeSystem.Interface;

namespace Wpf2Html5.TypeSystem.Items
{
    class GenericTypeRef : ItemBase, IVariableContext
    {
        private ITypeItem _ltype;
        private Type _rtype;

        public override Type RType
        {
            get { return _rtype; }
        }

        #region Construction

        public GenericTypeRef(IDeclarationContext parent, ITypeItem generic, Type instance)
            : base(parent, instance.GetRName())
        {
            _ltype = generic;
            _rtype = instance;
        }

        #endregion

        public ITypeItem GetVariable(string name)
        {
            var rtype = RType;

            var members = rtype.GetMember(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (members.Any())
            {
                if (members.Count() > 1)
                {
                    Log.Warning("member name '" + name + "' is ambigous.");
                    return null;
                }

                var options = TranslateOptions.None;

                var member = members.First();
                if (member is MethodInfo)
                {
                    var method = (MethodInfo)member;
                    if (method.ReturnType.IsGenericType)
                    { }
                    var returntype = ParentTypes.TranslateRType(method.ReturnType, options);
                    return new Method(this, name, returntype);
                }
                else if (member is PropertyInfo)
                {
                    var property = (PropertyInfo)member;
                    return new Property(this, name, ParentTypes.TranslateRType(property.PropertyType, options));
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else if(_ltype is IVariableContext)
            {
                // look in baseclass
                return ((IVariableContext)_ltype).GetVariable(name);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        public void AddVariable(string name, ITypeItem type)
        {
            throw new NotImplementedException();
        }

        public override bool Prepare()
        {
            if(GStatus == TypeGenerationStatus.initial)
            {
                GStatus = TypeGenerationStatus.cloaked;
                return true;
            }
            else if (GStatus == TypeGenerationStatus.cloaked)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /*private ITypeItem TranslateArgument(Type rtype)
        {
            if(rtype.IsGenericType)
            {

            }
            else
            {
                return ParentTypes.TranslateRType(rtype);
            }
        }*/ 
    }
}
