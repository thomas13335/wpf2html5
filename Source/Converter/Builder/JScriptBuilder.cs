using Jint.Parser;
using Jint.Parser.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wpf2Html5.Builder;
using Wpf2Html5.Converter;

namespace Wpf2Html5.Builder
{
    /// <summary>
    /// A JScript rewriter based on JInt.
    /// </summary>
    public class JScriptBuilder : JScriptWriter
    {
        #region Private Fields

        private Dictionary<Type, Action<object>> _map = new Dictionary<Type, Action<object>>();
        private bool _noeolstmt;
        private bool _skipeolonce;
        private ConverterStack _stack = new ConverterStack();
        private Dictionary<string, string> _idmap = new Dictionary<string, string>();
        private List<string> _suppressedglobalfunctions = new List<string>();

        #endregion

        #region Public Properties

        public IList<string> SuppressedGlobalFunctions { get { return _suppressedglobalfunctions; } }

        #endregion

        #region Construction

        public JScriptBuilder()
        { }

        #endregion

        #region Public Methods

        /// <summary>Rewrites the given program code.</summary>
        /// <param name="programcode">The jscript code to rewrite.</param>
        public void RewriteProgram(string programcode)
        {
            var parser = new JavaScriptParser();
            try
            {
                var ast = parser.Parse(programcode);
                Emit(ast);
            }
            catch(ParserException ex)
            {
                Log.Trace("jscript parsing error: {0}\n{1}", ex.Message, ex.Source);
                throw ex;
            }
        }

        /// <summary>Rewrites the program code contained in another writer or builder.</summary>
        /// <param name="writer">The writer to rewrite.</param>
        public void RewriteProgram(JScriptWriter writer)
        {
            RewriteProgram(writer.Text);
        }

        /// <summary>Emits code for an AST node.</summary>
        /// <param name="node"></param>
        public void Emit(object node)
        {
            Trace("node : " + node + " ...");

            if (node is Program)
            {
                EmitProgram(node as Program);
            }
            else if (node is EmptyStatement)
            { 
            }
            else if (node is ContinueStatement)
            {
                Write("continue;");
            }
            else if (node is BreakStatement)
            {
                Write("break;");
            }
            else if (node is Boolean)
            {
                Write(((bool)node).ToString().ToLower());
            }
            else if (node is Literal)
            {
                var literal = node as Literal;
                Write(literal.Raw);
            }
            else if (node == null)
            {

            }
            else
            {
                var nodetype = node.GetType();
                if (true)
                {
                    Action<object> emit;
                    if (!_map.TryGetValue(nodetype, out emit))
                    {
                        var method = GetType().GetMethod("Emit" + nodetype.Name, BindingFlags.NonPublic | BindingFlags.Instance);
                        if (null == method)
                        {
                            _map[nodetype] = null;
                            throw new Exception("don't know how to emit [" + nodetype.FullName + "]");
                        }
                        else
                        {
                            try
                            {
                                emit = p => method.Invoke(this, new object[] { p });
                                _map[nodetype] = emit;
                            }
                            catch(TargetInvocationException ex)
                            {
                                throw ex.InnerException;
                            }
                        }
                    }

                    if (null != emit)
                    {
                        emit(node);
                    }
                }
                else
                {
                    throw new Exception("don't know how to emit [" + nodetype.FullName + "]");
                }
            }

            if (node is Statement && !_noeolstmt && !_skipeolonce)
            {
                WriteLine();
            }

            _skipeolonce = false;
        }

        public void AddIdentifierTranslation(string name, string cname)
        {
            _idmap[name] = cname;
        }

        #endregion

        #region Expressions

        private void EmitConditionalExpression(ConditionalExpression e)
        {
            Emit(e.Test);
            Write(" ? ");
            Emit(e.Consequent);
            Write(" : ");
            Emit(e.Alternate);
        }

        private void EmitUnaryExpression(UnaryExpression e)
        {
            if (e.Prefix)
            {
                using (var ec = new Wpf2Html5.Builder.ExpressionContext(this, e))
                {
                    Write(ec.Token);
                    Emit(e.Argument);
                }
            }
            else
            {
                using (var ec = new Wpf2Html5.Builder.ExpressionContext(this, e))
                {
                    Emit(e.Argument);
                    Write(ec.Token);
                }
            }
        }

        private void EmitUpdateExpression(UpdateExpression e)
        {
            using (var ec = new ExpressionContext(this, e))
            {
                if (e.Prefix)
                {
                    Write(ec.Token);
                    Emit(e.Argument);
                }
                else
                {
                    Emit(e.Argument);
                    Write(ec.Token);
                }
            }
        }

        private void EmitBinaryExpression(BinaryExpression e)
        {
            using (var ec = new ExpressionContext(this, e))
            {
                Emit(e.Left);
                Write(" " + ec.Token + " ");
                Emit(e.Right);
            }
        }

        private void EmitLogicalExpression(LogicalExpression e)
        {
            using (var ec = new ExpressionContext(this, e))
            {
                Emit(e.Left);
                Write(" " + ec.Token + " ");
                Emit(e.Right);
            }
        }

        #endregion

        #region Try Catch

        private void EmitTryStatement(TryStatement s)
        {
            Write("try");
            Emit(s.Block);
            foreach (var handler in s.Handlers)
            {
                Write("catch(");
                Emit(handler.Param);
                Write(") ");
                Emit(handler.Body);
            }

            if(null != s.Finalizer)
            {
                Write("finally");
                Emit(s.Finalizer);
            }
        }

        private void EmitThrowStatement(ThrowStatement s)
        {
            Write("throw ");
            Emit(s.Argument);
            Write(";");
        }

        private void EmitForInStatement(ForInStatement s)
        {
            if (s.Each)
            {
                throw new NotImplementedException();
            }
            else
            {
                Write("for (");
                if (s.Left is VariableDeclaration)
                {
                    var decl = s.Left as VariableDeclaration;
                    Emit(decl.Declarations.First());
                }
                else
                {
                    Emit(s.Left);
                }
                Write(" in ");
                Emit(s.Right);
                Write(") ");
            }

            Emit(s.Body);
        }

        #endregion

        #region Private Methods

        private void EmitReturnStatement(ReturnStatement ret)
        {
            Write("return ");
            Emit(ret.Argument);
            Write(";");
        }

        private void EmitDoWhileStatement(DoWhileStatement s)
        {
            Write("do ");
            Emit(s.Body);
            Write("while(");
            Emit(s.Test);
            Write(");");
        }

        private void EmitWhileStatement(WhileStatement s)
        {
            _noeolstmt = true;
            Write("while(");
            Emit(s.Test);
            Write(") ");
            _noeolstmt = false;
            Emit(s.Body);
        }

        private void EmitForStatement(ForStatement s)
        {
            _noeolstmt = true;
            Write("for(");
            Emit(s.Init);
            if (!(s.Init is Statement)) { Write("; "); } else Write(" ");
            Emit(s.Test);
            Write("; ");
            Emit(s.Update);
            Write(")");
            _noeolstmt = false;
            Emit(s.Body);
        }

        private void EmitSwitchStatement(SwitchStatement s)
        {
            Write("switch(");
            Emit(s.Discriminant);
            Write(")");
            EnterBlock();
            foreach (var c in s.Cases)
            {
                Write(c.Test == null ? "default " : "case ");
                Emit(c.Test);
                WriteLine(": ");
                Indent();
                foreach (var stmt in c.Consequent)
                {
                    Emit(stmt);
                }
                UnIndent();
            }

            LeaveBlock();
        }

        private void EmitIfStatement(IfStatement s)
        {
            Write("if(");
            Emit(s.Test);
            Write(") ");
            Emit(s.Consequent);
            if (null != s.Alternate)
            {
                Write(" else ");
                Emit(s.Alternate);
            }
            _skipeolonce = true;
        }

        private void EmitArrayExpression(ArrayExpression e)
        {
            bool first = true;
            Write("[ ");
            foreach (var item in e.Elements)
            {
                if (first) first = false; else Write(", ");
                Emit(item);

            }
            Write("] ");
        }

        private void EmitObjectExpression(ObjectExpression e)
        {
            Write("{ ");
            bool first = true;
            foreach (var p in e.Properties)
            {
                if (first) first = false; else Write(", ");
                Emit(p.Key);
                Write(": ");
                Emit(p.Value);
            }
            Write("}");
        }

        private void EmitCallExpression(CallExpression e)
        {
            bool suppress = false;
            if(e.Callee is Identifier)
            {
                // global function call?
                var id = e.Callee as Identifier;
                if(_suppressedglobalfunctions.Contains(id.Name))
                {
                    suppress = true;
                }
            }

            if (!suppress)
            {
                Emit(e.Callee);
                EmitFunctionCallArguments(e.Arguments);
            }
            else
            {
                // skip this call statement.
            }
        }

        private void EmitNewExpression(NewExpression e)
        {
            Write("new ");
            Emit(e.Callee);
            EmitFunctionCallArguments(e.Arguments);
        }

        private void EmitFunctionCallArguments(IEnumerable<Expression> args)
        {
            Write("(");
            bool first = true;
            foreach (var arg in args)
            {
                if (first) first = false; else Write(", ");
                Emit(arg);
            }
            Write(")");

        }

        private void EmitVariableDeclaration(VariableDeclaration e)
        {
            Write(e.Kind + " ");
            bool first = true;
            foreach (var decl in e.Declarations)
            {
                if (first) first = false;
                else Write(", ");
                Emit(decl);
            }

            Write(";");
        }

        private void EmitVariableDeclarator(VariableDeclarator decl)
        {
            Emit(decl.Id);
            if (null != decl.Init)
            {
                Write(" = ");
                Emit(decl.Init);
            }
        }

        private void EmitFunctionExpression(FunctionExpression e)
        {
            if (null == e.Body)
            {
                Emit(e.Expression);
            }
            else
            {
                TranslateFunctionHeader(null, e.Parameters);
                Emit(e.Body);
            }
        }

        private void EmitIdentifier(Identifier identifier)
        {
            var name = identifier.Name;
            if (IsCompact)
            {
                string cname;
                if (_idmap.TryGetValue(name, out cname))
                {
                    Write(cname);
                }
                else
                {
                    Write(name);
                }
            }
            else
            {
                Write(name);
            }
        }

        private void EmitThisExpression(ThisExpression e)
        {
            Write("this");
        }

        private void EmitMemberExpression(MemberExpression e)
        {
            Emit(e.Object);
            if (e.Computed)
            {
                Write("[");
                Emit(e.Property);
                Write("]");
            }
            else
            {
                Write(".");
                Emit(e.Property);
            }
        }

        private void EmitAssignmentExpression(AssignmentExpression e)
        {
            using (var ec = new ExpressionContext(this, e))
            {
                Emit(e.Left);
                Write(" " + TranslateAssignmentOperator(e.Operator) + " ");
                Emit(e.Right);
            }
        }

        private void EmitExpressionStatement(ExpressionStatement s)
        {
            using (var ec = new ExpressionContext(this, OperatorInfo.Body))
            {
                if (s.Expression is CallExpression)
                {
                    var call = s.Expression as CallExpression;
                    if (call.Callee is FunctionExpression)
                    {
                        // group around function expression (function(){}());
                        ec.BeginGroup();
                    }
                }

                Emit(s.Expression);
            }

            Write(";");
        }

        private void EmitBlockStatement(BlockStatement block)
        {
            EnterBlock();

            using (var ex = new ExpressionContext(this, block))
            {

                foreach (var s in block.Body)
                {
                    Emit(s);
                }
            }

            LeaveBlock();
        }

        private void EmitFunctionDeclaration(FunctionDeclaration fdecl)
        {
            WriteSeparator();
            TranslateFunctionHeader(fdecl, fdecl.Parameters);

            using (var ec = new ExpressionContext(this, OperatorInfo.Body))
            {
                Emit(fdecl.Body);
            }
        }

        private void EmitLabelledStatement(LabelledStatement lstmt)
        {
            WriteSeparator();
            Write(lstmt.Label.Name + ":");
            WriteSeparator();
            Emit(lstmt.Body);
        }

        private void EmitSequenceExpression(SequenceExpression sex)
        {
            Write("(");
            var first = true;
            foreach(var e in  sex.Expressions)
            {
                if (first) first = false;
                else Write(", ");
                Emit(e);
            }
            Write(")");
        }

        private void EmitProgram(Program program)
        {
            // WriteSeparator();

            foreach(var statement in program.Body)
            {
                Emit(statement);
            }
        }

        #endregion

        #region Translations

        private string TranslateAssignmentOperator(AssignmentOperator op)
        {
            switch (op)
            {
                case AssignmentOperator.Assign: return "=";
                case AssignmentOperator.PlusAssign: return "+=";
                case AssignmentOperator.MinusAssign: return "-=";
                case AssignmentOperator.BitwiseOrAssign: return "|=";
                case AssignmentOperator.BitwiseAndAssign: return "&=";
                case AssignmentOperator.BitwiseXOrAssign: return "^=";
                case AssignmentOperator.UnsignedRightShiftAssign: return ">>>=";
                case AssignmentOperator.LeftShiftAssign: return "<<=";
                case AssignmentOperator.RightShiftAssign: return ">>=";
                case AssignmentOperator.ModuloAssign: return "%=";
                default:
                    throw new NotImplementedException("TODO: operator " + op + " is not supported.");
            }
        }

        private void TranslateFunctionHeader(FunctionDeclaration fdecl, IEnumerable<Identifier> parameters)
        {
            Write("function ");
            if (null != fdecl && null != fdecl.Id)
            {
                Emit(fdecl.Id);
            }

            Write("(");
            bool first = true;
            foreach (var p in parameters)
            {
                if (first) first = false; else Write(", ");
                Write(p.Name);
            }

            Write(") ");

        }


        #endregion
    }
}
