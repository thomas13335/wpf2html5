using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Linq;

namespace Wpf2Html5.Converter
{
    class DeclarationSourceContext
    {
        public UsingDirectiveSyntax[] Usings { get; private set; }

        public BaseTypeDeclarationSyntax Declaration { get; private set; }

        public ClassDeclarationSyntax ClassDeclaration { get { return (ClassDeclarationSyntax)Declaration; } }

        public DeclarationSourceContext(BaseTypeDeclarationSyntax declaration, IEnumerable<UsingDirectiveSyntax> usings)
        {
            Declaration = declaration;
            Usings = usings.ToArray();
        }
    }
}
