using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wpf2Html5.Converter;
using Wpf2Html5.Converter.Rewriter.Stack;
using Wpf2Html5.Exceptions;
using Wpf2Html5.TypeSystem;
using Wpf2Html5.TypeSystem.Interface;

namespace Wpf2Html5.Converter.Rewriter
{
    /// <summary>
    /// Rewriters C# code as jscript code.
    /// </summary>
    class CSharpToJScriptRewriter : CSharpSyntaxRewriter
    {
        #region Private Fields

        private IDeclarationContext _context;
        private DeclarationEmitContext _dgc;
        private RewriterStack _stack = new RewriterStack();
        private List<string> _usingprefixes;

        #endregion

        public CSharpToJScriptRewriter(DeclarationEmitContext dgc, IDeclarationContext context)
        {
            _dgc = dgc;
            _context = context;
            _usingprefixes = new List<string>();
            if (null != _dgc)
            {
                _usingprefixes.AddRange(dgc.Declaration.GetContainingNamespaces());
                _usingprefixes.AddRange(_dgc.Usings.Select(u => u.Name.ToString()).ToList());
            }
        }

        #region Diagnostics

        public static bool Verbose = false;

        private void Trace(string format, params object[] args)
        {
            if (Verbose)
            {
                Log.Trace("rewriter: " + format, args);
            }
        }

        private void Warning(string format, params object[] args)
        {
            if (Verbose)
            {
                Debug.WriteLine("WARNING: rewrite: " + format, args);
            }
        }

        #endregion

        #region Stack

        private T GetCurrent<T>()
        {
            return _stack.OfType<T>().FirstOrDefault();
        }

        private T GetTop<T>() where T : class
        {
            return _stack.Count == 0 ? null : _stack.Peek() as T;
        }

        #endregion

        #region Variables

        private IVariableContext GetVariableContext()
        {
            var top = GetCurrent<IVariableContext>();
            return null == top ? _context : top;
        }

        private IDeclarationContext Context
        {
            get
            {
                var top = GetCurrent<IDeclarationContext>();
                return null == top ? _context : top;
            }
        }

        private ITypeItem GetVariable(string name)
        {
            return GetVariableContext().GetVariable(name);
        }

        #endregion

        #region Type Annotations

        private ITypeItem GetLType(Type rtype)
        {
            return Context.TranslateRType(rtype, TranslateOptions.MustExist);
        }

        private ITypeItem GetLType(SyntaxNode node)
        {
            Trace("looking for annotation : {0}", node);
            SyntaxAnnotation annotation = null;
            var childnode = node;
            while (null != childnode)
            {
                annotation = childnode.GetAnnotations("ltype").LastOrDefault();
                if (null != annotation)
                {
                    Trace("  found annotation @{0} => {1}", childnode, annotation.Data);
                    break;
                }

                childnode = childnode.ChildNodes().FirstOrDefault();
            }

            if (null != annotation)
            {
                return Context.ResolveLType(annotation.Data);
            }
            else
            {
                return null;
            }
        }

        private T AnnotateLType<T>(T node, ITypeItem ltype) where T : SyntaxNode
        {
            if (null == ltype)
            {
                return node;
            }


            T result;
            var key = ltype.ID;

            var existing = node.GetAnnotations("ltype").FirstOrDefault();
            if (null != existing)
            {
                if (existing.Data != key)
                {
                }
                else
                {
                    return node;
                }
            }

            Trace("annotate: {0,-30} {1}", node, ltype);

            result = node
                .WithAdditionalAnnotations(new SyntaxAnnotation("ltype", ltype.ID));

            var check = GetLType(result);

            return result;
        }

        #endregion

        #region Public Methods

        public void AddMethodParameters(IEnumerable<ParameterSyntax> parameters)
        {
            if (null != parameters)
            {
                foreach (var par in parameters)
                {
                    var ltype = ResolveTypeName(par.Type);
                    _context.AddVariable(par.Identifier.ToString(), ltype);
                }
            }

        }

        #endregion

        #region Visitor Overrides

        public override SyntaxNode VisitThisExpression(ThisExpressionSyntax node)
        {
            var item = _context.GetVariable("this");

            //var thisname = null != GetCurrent<LambdaContext>() ? "__this" : "this";

            SyntaxNode result = SyntaxFactory.IdentifierName(CSharpToJScriptConverter.Settings.OuterThis);

            result = AnnotateLType(result, item.LType);

            return result;
        }

        public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
        {
            if (null != GetTop<MemberContextEntry>())
            {
                // in a member access context, don't translate.
                return node;
            }
            else
            {
                ExpressionSyntax result = node;

                // the logical name of the variable
                var name = node.Identifier.ValueText;
                if (name == CSharpToJScriptConverter.Settings.OuterThis)
                {
                    name = "this";
                }

                // look for a variable with the given name
                var item = GetVariable(name);
                ITypeItem ltype;
                if (null == item)
                {
                    // look if there is a type with that name
                    var syn = SyntaxFactory.IdentifierName(name);
                    if (null != (item = ResolveTypeName(syn)))
                    {
                        ltype = item;
                    }
                    else
                    {
                        throw new Exception("unresolved variable '" + name + "'.");
                    }
                }
                else
                {
                    ltype = item.LType;

                    if (item.IsMember && name != "this")
                    {
                        //var thisname = null != GetCurrent<LambdaContext>() ? "__this" : "this";
                        result = (ExpressionSyntax)
                            SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName(CSharpToJScriptConverter.Settings.OuterThis),
                                node);

                        result = Visit(result) as ExpressionSyntax;
                    }
                }

                Trace("identifier: {0} -> {1} [{2}]", node, result, ltype);

                if (null == ltype)
                {
                    ltype = GetLType(typeof(object));
                }

                return AnnotateLType(result, ltype);
            }
        }

        public override SyntaxNode VisitCastExpression(CastExpressionSyntax node)
        {
            var type = ResolveTypeName(node.Type);
            if (null == type)
            {
                Warning("cast type [" + node.Type + "] not found.");
            }

            var result = Visit(node.Expression);

            if (null != type)
            {
                result = AnnotateLType(result, type);
            }

            return result;
        }

        public override SyntaxNode VisitLiteralExpression(LiteralExpressionSyntax node)
        {
            ITypeItem ltype = null;

            if (node.Token.IsKeyword())
            {
                var keyword = node.ToString();
                if (keyword == "false" || keyword == "true")
                {
                    ltype = GetLType(typeof(bool));
                }
                else if (keyword == "null")
                {
                    // NULL handling: no type!
                    ltype = null;
                }
                else
                {
                    throw new RewriterException(node, ErrorCode.NotSupported,
                        "keyword '" + node + "' not converted.");
                }
            }
            else if (node.Token.IsKind(SyntaxKind.NumericLiteralToken))
            {
                ltype = GetLType(typeof(int));
            }
            else if (node.Token.IsKind(SyntaxKind.StringLiteralToken))
            {
                ltype = GetLType(typeof(string));
            }
            else
            {
                throw new RewriterException(node, ErrorCode.NotSupported,
                    "literal expression '" + node + "' not converted.");
            }

            var result = base.VisitLiteralExpression(node);
            return AnnotateLType(result, ltype);
        }

        public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            ExpressionSyntax left;

            if (null != GetTop<LValueContext>())
            {
                return SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, Visit(node.Expression) as ExpressionSyntax, node.Name);
            }

            using (var sc = new RewriterStackContext<MemberContextEntry>(_stack, new MemberContextEntry()))
            {
                // get list of nested member access expressions in reverse order
                var maclist = node.DescendantNodesAndSelf().OfType<MemberAccessExpressionSyntax>().Reverse().ToList();

                // left child of inner most member access: resolution context
                left = maclist.First().Expression;


                using (var esc = new RewriterStackContext<ExpressionContextEntry>(_stack, new ExpressionContextEntry()))
                {
                    // visit the leftmost item (not a member access)
                    left = Visit(left) as ExpressionSyntax;

                    var ltype = GetLType(left);
                }

                // extract type annotation ...
                if (null == (sc.Entry.Left = GetLType(left)))
                {
                    Warning("unresolved type '{0}' in '{1}'.", left, node);
                }

                // iterate elements of the member access expression.
                foreach (var right in maclist)
                {
                    var membername = right.Name.ToString();

                    // item associated with the container
                    var container = sc.Entry.Left;

                    // type to the right
                    ITypeItem righttype = null;
                    ITypeItem item = null;

                    if (null != container)
                    {
                        if (null != (item = container.GetMember(membername)))
                        {
                            righttype = item.LType;
                        }
                        else
                        {
                            throw new RewriterException(node, ErrorCode.UnresolvedMember, 
                                "member '" + membername + "' not found in " + container);
                        }
                    }
                    else
                    {
                        Warning("no left side for '{0}'.", membername);
                    }

                    if (null == container)
                    {
                        left = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            left,
                            right.Name);
                    }
                    else if (TranslateMemberName(container, ref membername))
                    {
                        left = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            left,
                            SyntaxFactory.IdentifierName(membername));
                    }
                    else if (container.IsEnumeration())
                    {
                        // enumeration constants convert to string
                        left = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(membername));
                        // righttype = _context.GetLType(typeof(string));
                    }
                    else
                    {
                        if (null == item)
                        {
                            // Warning("type '" + container.ID + "' does not define member '" + membername + "'.");
                            left = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                left,
                                SyntaxFactory.IdentifierName(membername));

                            sc.Entry.Left = null;
                        }
                        else if (item.IsProperty())
                        {
                            var accessor = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                left,
                                SyntaxFactory.IdentifierName("get_" + membername));

                            left = SyntaxFactory.InvocationExpression(accessor);
                        }
                        else if (item.LType != null && item.LType.ID == "System.Windows.DependencyProperty")
                        {
                            // use a PropertyName as the DependencyProperty constant.
                            var dpname = membername.Substring(0, membername.Length - 8);
                            left = SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(dpname));
                            sc.Entry.Left = null;
                        }
                        else if (item.LType != null && item.LType.IsDelegate)
                        {
                            left = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                left,
                                SyntaxFactory.IdentifierName("fire_" + membername));

                        }
                        else
                        {
                            left = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                left,
                                SyntaxFactory.IdentifierName(membername));
                        }

                        if (null != item)
                        {
                            sc.Entry.Left = item.LType;
                        }
                        else
                        {
                            sc.Entry.Left = null;
                        }
                    }

                    if (null != righttype)
                    {
                        left = AnnotateLType(left, righttype) as ExpressionSyntax;
                    }
                    else
                    {
                        //Warning("no type for member '{0}' in '{1}', assume object [{2}].", membername, left, container);
                        throw new RewriterException(node, ErrorCode.RewriterError, "member '" + membername + "' no type definition found.");
                    }
                }

                /*if (sc.Entry.Left != null)
                {
                    left = AnnotateLType(left, sc.Entry.Left) as ExpressionSyntax;
                }*/

                Trace("member access: {0} -> {1} [{2}]", node, left, GetLType(left));
            }

            return left;
        }

        public override SyntaxNode VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            ExpressionSyntax left;
            ITypeItem ltype = null;

            using (var ct = new RewriterStackContext<LValueContext>(_stack, new LValueContext()))
            {
                left = Visit(node.Left) as ExpressionSyntax;
                ltype = GetLType(left);
            }

            var op = node.OperatorToken.ToString();
            var right = Visit(node.Right) as ExpressionSyntax;

            if (left is MemberAccessExpressionSyntax)
            {
                var member = left as MemberAccessExpressionSyntax;

                left = Visit(member.Expression) as ExpressionSyntax;

                var name = member.Name;
                if (null == (ltype = GetLType(left)))
                {
                    throw new Exception("type of '" + left + "' is unknown.");
                }

                var variable = ltype.GetMember(name.ToString());

                if (null == variable)
                {
                    Warning("member '{0}' not defined in [{1}].", name, ltype);

                    var newmember = SyntaxFactory
                        .MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, left, name);

                    left = SyntaxFactory
                        .AssignmentExpression(node.Kind(), newmember, right);

                }
                else if (variable.IsProperty())
                {
                    if (node.Kind() != SyntaxKind.SimpleAssignmentExpression)
                    {
                        throw new NotImplementedException();
                    }

                    // convert to property-set access
                    var accessor = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        left,
                        SyntaxFactory.IdentifierName("set_" + name));

                    var list = new List<ArgumentSyntax>();
                    list.Add(SyntaxFactory.Argument(right));

                    left = SyntaxFactory.InvocationExpression(accessor,
                        SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(list)));
                }
                else if (variable.LType.IsDelegate)
                {
                    if (op == "+=")
                    {
                        // convert to 'add' call
                        var accessor = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            left,
                            SyntaxFactory.IdentifierName("add_" + name));

                        var list = new List<ArgumentSyntax>();
                        list.Add(SyntaxFactory.Argument(right));

                        left = SyntaxFactory.InvocationExpression(accessor,
                            SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(list)));

                    }
                    else if (op == "-=")
                    {
                    }
                    else
                    {
                        throw new InvalidOperationException();
                    }
                }
                else
                {
                    left = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        left,
                        name);

                    AnnotateLType(left, variable.LType);

                    left = SyntaxFactory.AssignmentExpression(node.Kind(), left, right);
                }

                return left;
            }
            else
            {
                left = Visit(left) as ExpressionSyntax;
                var result = SyntaxFactory.AssignmentExpression(node.Kind(), left, right);

                return AnnotateLType(result, ltype);
            }
        }

        public override SyntaxNode VisitVariableDeclaration(VariableDeclarationSyntax node)
        {
            var visited = new List<VariableDeclaratorSyntax>();
            var context = GetVariableContext();

            ITypeItem lefttype = null;

            // resolve declaration type
            if (node.Type.ToString() != "var")
            {
                lefttype = ResolveTypeName(node.Type);
            }

            foreach (var declarator in node.DescendantNodes().OfType<VariableDeclaratorSyntax>())
            {
                var newdecl = Visit(declarator) as VariableDeclaratorSyntax;
                var name = newdecl.Identifier.ToString();
                var initializer = newdecl.Initializer;
                var righttype = (null == initializer ? null : GetLType(initializer)) ?? lefttype;

                if (righttype == null)
                {
                    throw new RewriterException(node, ErrorCode.RewriterError, "type for '" + node + "' not resolved.");
                }

                Trace("declare variable '{0}' type '{1}' in [{2}].", name, righttype.ID, context);

                context.AddVariable(name, righttype);
                AnnotateLType(newdecl, righttype);
                visited.Add(newdecl);

            }

            var list = SyntaxFactory.SeparatedList<VariableDeclaratorSyntax>(visited);
            var nt = SyntaxFactory.ParseTypeName("var");

            return SyntaxFactory.VariableDeclaration(nt.WithTrailingTrivia(SyntaxFactory.Space), list);
        }

        public override SyntaxNode VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            SyntaxNode result;
            ITypeItem ltype;

            var first = node.Expression;
            var maccess = first as MemberAccessExpressionSyntax;
            if (null == maccess)
            {
                result = base.VisitInvocationExpression(node);
            }
            else
            {
                if (maccess.Expression.ToString() == "base")
                {
                    ltype = _context.GetVariable("base");
                    var args = new List<ArgumentSyntax>();
                    args.Add(SyntaxFactory.Argument(SyntaxFactory.IdentifierName("this")));
                    args.AddRange(node.ArgumentList.Arguments);

                    var method = ltype.RType.Name + ".prototype." + maccess.Name + ".call";

                    result = SyntaxFactory.InvocationExpression(
                        SyntaxFactory.IdentifierName(method),
                        SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList<ArgumentSyntax>(args)));
                }
                else
                {
                    result = base.VisitInvocationExpression(node);
                }
            }

            ltype = GetLType(result);
            result = AnnotateLType(result, ltype);

            //Trace("invc: resulttype: {0}", GetLType(result));

            return result;
        }

        public override SyntaxNode VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            SyntaxNode result;
            ITypeItem ltype = null;

            if (node.OperatorToken.ToString() == "is")
            {
                // type 'is' operator
                var left = (ExpressionSyntax)Visit(node.Left);
                var ltypecmp = GetLType(Visit(node.Right));

                var args = new List<ArgumentSyntax>();
                args.Add(SyntaxFactory.Argument(left));
                args.Add(SyntaxFactory.Argument(SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, SyntaxFactory.Literal(ltypecmp.CodeName))));

                result = SyntaxFactory.InvocationExpression(
                    SyntaxFactory.IdentifierName("TypeSystem.IsOfType"),
                    SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(args)));

                // returns bool
                ltype = GetLType(typeof(bool));
            }
            else
            {
                result = base.VisitBinaryExpression(node);

                // derive type ...
                ltype = GetLType(result);
            }

            result = AnnotateLType(result, ltype);
            return result;
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node)
        {
            var methodcontext = ModuleFactory.CreateMethodContext(Context);
            using (var sc = new RewriterStackContext<IMethodContext>(_stack, methodcontext))
            {
                return base.VisitMethodDeclaration(node);
            }
        }

        public override SyntaxNode VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            return TranslateLambdaExpression(node.ParameterList, node.Body);
        }

        public override SyntaxNode VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
            var seplist = SyntaxFactory.SeparatedList<ParameterSyntax>(new[] { node.Parameter });
            var parlist = SyntaxFactory.ParameterList(seplist);
            return TranslateLambdaExpression(parlist, node.Body);
        }

        public override SyntaxNode VisitBlock(BlockSyntax node)
        {
            SyntaxNode result;
            using (var sc = new RewriterStackContext<BlockContext>(_stack, new BlockContext()))
            {
                result = base.VisitBlock(node);
                result = result.WithAdditionalAnnotations(new SyntaxAnnotation("haslambdas", "1"));
            }

            return result;
        }

        public override SyntaxNode VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            var ltype = ResolveTypeName(node.Type);

            var args = node.ChildNodes()
                .Select(n => Visit(n))
                .OfType<ArgumentListSyntax>()
                .FirstOrDefault()
                ??
                SyntaxFactory.ArgumentList();

            var inits = node.ChildNodes().OfType<InitializerExpressionSyntax>().FirstOrDefault();
            if (null != inits)
            {
                throw new RewriterException(node, ErrorCode.NotSupported, "initializers are not supported.");
            }

            var jtype = SyntaxFactory.ParseTypeName(ltype.CodeName).WithLeadingTrivia(SyntaxFactory.Space);
            var result = SyntaxFactory.ObjectCreationExpression(jtype, args, inits);

            result = AnnotateLType(result, ltype);

            return result;
        }

        public override SyntaxNode VisitCatchDeclaration(CatchDeclarationSyntax node)
        {
            SyntaxTreeHelper.PrintTree(node);
            return base.VisitCatchDeclaration(node);
        }

        /// <summary>
        /// Converts the statement into a IEnumerator based iteration.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public override SyntaxNode VisitForEachStatement(ForEachStatementSyntax node)
        {
            // Type, Identifier, Expression

            // transform the enumeration source expression.
            var cnode = Visit(node.Expression);
            var expr = (ExpressionSyntax)cnode;

            // type item of the source
            var sourcetype = GetLType(expr);

            // 'GetEnumerator' method on the source (don't care about interfaces here)
            var getenum = sourcetype.GetMember("GetEnumerator");
            if (null == getenum)
            {
                throw new RewriterException(node, ErrorCode.NotEnumerable, "type " + sourcetype + " has no 'GetEnumerator' method.");
            }

            var elementtype = getenum.LType;

            var it = SyntaxFactory.IdentifierName("$it");

            var getenumexpr = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    expr,
                    SyntaxFactory.IdentifierName("GetEnumerator")
                )
            );

            var itdecl = CreateVariableDeclaration(it.Identifier, getenumexpr, SyntaxFactory.IdentifierName(elementtype.ID));

            var cond = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    it, SyntaxFactory.IdentifierName("MoveNext")),
                    SyntaxFactory.ArgumentList());

            var innerlist = new List<StatementSyntax>();


            var current =
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    it, SyntaxFactory.IdentifierName("get_Current")),
                    SyntaxFactory.ArgumentList()
                );

            var blocklist = new List<StatementSyntax>();
            blocklist.Add(CreateVariableDeclaration(node.Identifier, current));

            if (node.Statement is BlockSyntax)
            {
                blocklist.AddRange(((BlockSyntax)node.Statement).Statements);
            }
            else
            {
                blocklist.Add(node.Statement);
            }

            var wh = SyntaxFactory.WhileStatement(cond, SyntaxFactory.Block(blocklist));


            var result = SyntaxFactory.Block(
                itdecl,
                wh);

            //Log.Trace("visiting generated code:\n{0}", result);

            var output = VisitBlock(result);

            // SyntaxTreeHelper.PrintTree(output);

            return output;
        }

        #endregion

        private StatementSyntax CreateVariableDeclaration(SyntaxToken name, ExpressionSyntax init, TypeSyntax type = null)
        {
            type = type ?? SyntaxFactory.IdentifierName("var");
            var ittype = type.WithTrailingTrivia(SyntaxFactory.Whitespace(" "));
            VariableDeclaratorSyntax declarator;
            if (null == init)
            {
                declarator = SyntaxFactory.VariableDeclarator(name);
            }
            else
            {
                declarator = SyntaxFactory.VariableDeclarator(name, null, SyntaxFactory.EqualsValueClause(init));
            }

            var itdecl = SyntaxFactory.VariableDeclaration(ittype, SyntaxFactory.SeparatedList(new[] { declarator }));
            return SyntaxFactory.LocalDeclarationStatement(itdecl);
        }

        #region Translation

        protected virtual bool TranslateMemberName(ITypeItem container, ref string membername)
        {
            if (membername == "ToString")
            {
                membername = "toString";
                return true;
            }
            else if (membername == "Count")
            {
                if (container.ID == "List")
                {
                    membername = "length";
                    return true;
                }
            }

            return false;
        }

        private SyntaxNode TranslateLambdaExpression(ParameterListSyntax parameters, SyntaxNode body)
        {
            using (var sc = new RewriterStackContext<LambdaContext>(_stack, new LambdaContext()))
            {
                var previous = GetVariableContext();
                var mc = ModuleFactory.CreateMethodContext(_context);
                using (var sc1 = new RewriterStackContext<IMethodContext>(_stack, mc))
                {
                    mc = sc1.Entry;
                    mc.AddMethodParameters(parameters.ChildNodes().OfType<ParameterSyntax>());
                    body = Visit(body);
                }
            }

            var ps = parameters.ChildNodes().Select(p => p.ToString()).ToSeparatorList();

            var bodysection = body.ToString();
            if (!(body is BlockSyntax))
            {
                bodysection = "{ " + bodysection + " }";
            }

            var text = " function (" + ps + ") " + bodysection;

            var block =
            GetCurrent<BlockContext>();
            if (null != block)
            {
                block.ContainsLambdas = true;
            }

            return SyntaxFactory.IdentifierName(text).WithAdditionalAnnotations(new SyntaxAnnotation("translated", "1"));
        }

        private string ConvertGenericNameSyntax(TypeSyntax gname, string id = null)
        {
            var sb = new StringBuilder();

            if (gname is GenericNameSyntax)
            {
                var generic = (GenericNameSyntax)gname;
                id = id ?? generic.Identifier.ToString();
                sb.Append(id);

                var args = generic.TypeArgumentList.Arguments;

                if (!id.Contains('`'))
                {
                    sb.Append("`");
                    sb.Append(args.Count);
                }

                sb.Append("<");

                sb.Append(args.Select(a => ResolveTypeName(a).ID).ToSeparatorList());

                sb.Append(">");
            }
            else
            {
                sb.Append(gname.ToString());
            }

            return sb.ToString();
        }

        private ITypeItem ResolveTypeName(TypeSyntax typesyntax)
        {
            var basename = typesyntax.ToString();
            GenericNameSyntax gname = null;

            if (typesyntax is GenericNameSyntax)
            {
                gname = (GenericNameSyntax)typesyntax;
                basename = gname.Identifier.ToString() + "`" + gname.TypeArgumentList.Arguments.Count;
            }

            // try with no prefix against the current context ...
            var result = _context.ResolveLType(basename);

            // enumerate usings
            if (null == result && null != _usingprefixes)
            {
                foreach (var prefix in _usingprefixes)
                {
                    var fullname = null == prefix ? basename : prefix + "." + basename;
                    var type = _context.ResolveLType(fullname);

                    if (null != type)
                    {
                        _dgc.TriggerItemReferenced(type);
                        result = type;
                        break;
                    }
                }
            }

            if (null == result)
            {
                throw new RewriterException(typesyntax, ErrorCode.UnresolvedTypeName,
                    "type '" + basename + "' could not be resolved.");
            }

            if (null != gname)
            {
                // set type parameters and resolve again
                var name = ConvertGenericNameSyntax(gname, result.ID);
                var gresult = _context.ResolveLType(name);

                if (null == gresult)
                {
                    // synthesize the generic type
                    var generictype = result.RType;
                    var argltypes = gname.TypeArgumentList.Arguments.Select(e => ResolveTypeName(e)).ToList();
                    var argrtypes = argltypes.Select(e => e.RType).ToArray();
                    var gtype = generictype.MakeGenericType(argrtypes);

                    // and register it
                    gresult = Context.TranslateRType(gtype, TranslateOptions.Add);
                }

                result = gresult;
            }

            if (Log.ShowTypeResolution)
            {
                Log.Trace("type resolved [{0}] => [{1}].", basename, result.ID);
            }

            return result;
        }

        #endregion

    }
}
