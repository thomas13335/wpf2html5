using Jint.Parser.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Wpf2Html5.Builder
{
    class ExpressionContext : IDisposable
    {
        private static ThreadLocal<ExpressionContext> _current = new ThreadLocal<ExpressionContext>();
        private ExpressionContext _previous;
        private OperatorInfo _op;
        private int _group;
        private JScriptBuilder _builder;

        public int Precedence { get { return _op.Precedence; } }

        public string Token { get { return _op.Token; } }

        public static ExpressionContext Current { get { return _current.Value; } }

        public ExpressionContext(JScriptBuilder builder)
        {
            _builder = builder;
            _previous = _current.Value;
            _current.Value = this;
        }

        public ExpressionContext(JScriptBuilder builder, OperatorInfo op)
            : this(builder)
        {
            _op = op;
            Enter();
        }

        public ExpressionContext(JScriptBuilder builder, UnaryExpression e)
            : this(builder)
        {
            _op = OperatorInfo.GetUnary(e.Operator, e.Prefix);
            Enter();
        }


        public ExpressionContext(JScriptBuilder builder, BinaryExpression e)
            : this(builder)
        {
            _op = OperatorInfo.GetBinary(e.Operator);
            Enter();
        }

        public ExpressionContext(JScriptBuilder builder, LogicalExpression e)
            : this(builder)
        {
            _op = OperatorInfo.GetLogical(e.Operator);
            Enter();
        }

        public ExpressionContext(JScriptBuilder builder, AssignmentExpression e)
            : this(builder)
        {
            _op = OperatorInfo.Assignment;
            Enter();
        }

        public ExpressionContext(JScriptBuilder builder, BlockStatement e)
            : this(builder)
        {
            _op = OperatorInfo.Block;
        }

        public void Dispose()
        {
            _current.Value = _previous;
            Leave();
        }

        private void Enter()
        {
            if (_previous != null && _previous.Precedence > this.Precedence)
            {
                _group++;
                _builder.Write("(");
            }
        }

        private void Leave()
        {
            while (_group-- > 0)
            {
                _builder.Write(")");
            }
        }


        internal void BeginGroup()
        {
            _group++;
            _builder.Write("(");
        }
    }
}
