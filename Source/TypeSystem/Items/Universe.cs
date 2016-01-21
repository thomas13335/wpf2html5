using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf2Html5.TypeSystem.Interface;

namespace Wpf2Html5.TypeSystem.Items
{
    /// <summary>
    /// Ultimate root of every type system, catches parent calls.
    /// </summary>
    class Universe : IDeclarationContext
    {
        public IDeclarationContext GetTypeDeclarationContext()
        {
            throw Failure();
        }

        public ITypeItem TranslateRType(Type rtype, TranslateOptions options = TranslateOptions.None)
        {
            throw Failure();
        }

        public ITypeItem ResolveLType(string name)
        {
            throw Failure();
        }

        public void AddDepdendency(ITypeItem dependent, ITypeItem target)
        {
            throw Failure();
        }

        public void RegisterType(ITypeItem ltype)
        {
            throw Failure();
        }

        public ITypeItem GetVariable(string name)
        {
            throw Failure();
        }

        public void AddVariable(string name, ITypeItem type)
        {
            throw Failure();
        }

        private Exception Failure()
        {
            return new InvalidOperationException("the universe has no declarations.");
        }
    }
}
