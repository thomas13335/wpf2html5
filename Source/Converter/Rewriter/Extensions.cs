using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf2Html5.TypeSystem;
using Wpf2Html5.TypeSystem.Interface;

namespace Wpf2Html5.Converter.Rewriter
{
    static class Extensions
    {
        public static ExpressionSyntax WithLTypeAnnotation(this ExpressionSyntax node, ITypeItem ltype)
        {
            if (null != ltype)
            {
                return node.WithAdditionalAnnotations(new SyntaxAnnotation("ltype", ltype.ID));
            }
            else
            {
                return node;
            }
        }

        public static void AddMethodParameters(this IDeclarationContext context, IEnumerable<ParameterSyntax> parameters)
        {
            if (null != parameters)
            {
                foreach (var par in parameters)
                {
                    // Trace("parameter {0}", par);
                    // SyntaxTreeHelper.PrintTree(par);

                    ITypeItem ltype = null;
                    string typename = null;
                    if (null != par.Type)
                    {
                        typename = par.Type.ExtractTypeName();
                        ltype = context.ResolveLType(typename);
                    }

                    if (null == ltype && null != par.Type)
                    {
                        // TraceTarget.Trace("WARNING: unresolved parameter type '{0}'.", typename);
                    }

                    context.AddVariable(par.Identifier.ToString(), ltype);
                }
            }
        }

        /// <summary>
        /// Strips generic type parameters from the type name.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static string ExtractTypeName(this TypeSyntax node)
        {
            var gensyn = node as GenericNameSyntax;
            if (null != gensyn) return gensyn.Identifier.ToString();
            else return node.ToString();
        }

        public static IEnumerable<string> GetContainingNamespaces(this SyntaxNode node)
        {
            var list = new List<string>();
            foreach (var ns in node.Ancestors().OfType<NamespaceDeclarationSyntax>())
            {
                var name = ns.ChildNodes().First().ToString();
                list.AddRange(name.Split('.'));
            }

            for(int j = 1; j <= list.Count; ++j)
            {
                yield return list.Take(j).ToSeparatorList(".");
            }
        }
    }
}
