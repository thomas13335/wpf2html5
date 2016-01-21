using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Wpf2Html5.Builder;
using Wpf2Html5.TypeSystem.Interface;

namespace Wpf2Html5.Converter
{
    /// <summary>
    /// Context for emitting a class declaration.
    /// </summary>
    class DeclarationEmitContext
    {
        #region Private

        private DeclarationSourceContext _dsc;
        private List<DependencyProperty> _dplist = new List<DependencyProperty>();

        #endregion

        #region Properties

        public ITypeItem LType { get; private set; }

        public BaseTypeDeclarationSyntax Declaration { get { return _dsc.Declaration; } }

        public DeclarationSourceContext Source { get { return _dsc; } }

        public UsingDirectiveSyntax[] Usings { get { return _dsc.Usings; } }

        public Action<ITypeItem> OnItemReferenced { get; set; }

        public JScriptWriter Writer { get; private set; }

        public IEnumerable<DependencyProperty> DependencyPropertyInitializations { get { return _dplist; } }

        #endregion

        public DeclarationEmitContext(ITypeItem type, DeclarationSourceContext dsc)
        {
            _dsc = dsc;
            LType = type;
            Writer = new JScriptWriter();
        }

        public void AddItemReferences()
        {
            foreach(var d in LType.Dependencies.Select(e => e.Target))
            {
                TriggerItemReferenced(d);
            }
        }

        public void AddDependencyPropertyInitialization(DependencyProperty dp)
        {
            _dplist.Add(dp);
        }

        public void TriggerItemReferenced(ITypeItem d)
        {
            if(null != OnItemReferenced)
            {
                OnItemReferenced(d);
            }
        }
    }
}
