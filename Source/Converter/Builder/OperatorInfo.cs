using Jint.Parser.Ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wpf2Html5.Builder
{
    class OperatorInfo
    {
        public string Token { get; private set; }

        public int Precedence { get; private set; }

        static Dictionary<UnaryOperator, OperatorInfo> _unaryprefix= new Dictionary<UnaryOperator, OperatorInfo>();
        static Dictionary<UnaryOperator, OperatorInfo> _unarypostfix = new Dictionary<UnaryOperator, OperatorInfo>();
        static Dictionary<BinaryOperator, OperatorInfo> _binary = new Dictionary<BinaryOperator, OperatorInfo>();
        static Dictionary<LogicalOperator, OperatorInfo> _logical = new Dictionary<LogicalOperator, OperatorInfo>();

        public static OperatorInfo Body = new OperatorInfo(null, 1);

        static OperatorInfo()
        {
            // precedence according to:
            // https://developer.mozilla.org/en-US/docs/Web/JavaScript/Reference/Operators/Operator_Precedence

            _unarypostfix[UnaryOperator.Increment] = new OperatorInfo("++", 16);
            _unarypostfix[UnaryOperator.Decrement] = new OperatorInfo("--", 16);

            _unaryprefix[UnaryOperator.Increment] = new OperatorInfo("++", 15);
            _unaryprefix[UnaryOperator.Decrement] = new OperatorInfo("--", 15);
            _unaryprefix[UnaryOperator.LogicalNot] = new OperatorInfo("!", 15);
            _unaryprefix[UnaryOperator.BitwiseNot] = new OperatorInfo("~", 15);
            _unaryprefix[UnaryOperator.Plus] = new OperatorInfo("+", 15);
            _unaryprefix[UnaryOperator.Minus] = new OperatorInfo("-", 15);
            _unaryprefix[UnaryOperator.Delete] = new OperatorInfo("delete ", 15);
            _unaryprefix[UnaryOperator.TypeOf] = new OperatorInfo("typeof ", 15);
            _unaryprefix[UnaryOperator.Void] = new OperatorInfo("void ", 15);

            _binary[BinaryOperator.Times] = new OperatorInfo("*", 14);
            _binary[BinaryOperator.Divide] = new OperatorInfo("/", 14);
            _binary[BinaryOperator.Modulo] = new OperatorInfo("%", 14);

            _binary[BinaryOperator.Plus] = new OperatorInfo("+", 13);
            _binary[BinaryOperator.Minus] = new OperatorInfo("-", 13);

            _binary[BinaryOperator.LeftShift] = new OperatorInfo("<<", 12);
            _binary[BinaryOperator.RightShift] = new OperatorInfo(">>", 12);
            _binary[BinaryOperator.UnsignedRightShift] = new OperatorInfo(">>>", 12);

            _binary[BinaryOperator.Less] = new OperatorInfo("<", 11);
            _binary[BinaryOperator.LessOrEqual] = new OperatorInfo("<=", 11);
            _binary[BinaryOperator.Greater] = new OperatorInfo(">", 11);
            _binary[BinaryOperator.GreaterOrEqual] = new OperatorInfo(">=", 11);
            _binary[BinaryOperator.InstanceOf] = new OperatorInfo("instanceof", 11);

            _binary[BinaryOperator.Equal] = new OperatorInfo("==", 10);
            _binary[BinaryOperator.NotEqual] = new OperatorInfo("!=", 10);

            _binary[BinaryOperator.StrictlyEqual] = new OperatorInfo("===", 10);
            _binary[BinaryOperator.StricltyNotEqual] = new OperatorInfo("!==", 10);

            _binary[BinaryOperator.BitwiseAnd] = new OperatorInfo("&", 9);
            _binary[BinaryOperator.BitwiseXOr] = new OperatorInfo("^", 8);
            _binary[BinaryOperator.BitwiseOr] = new OperatorInfo("|", 7);

            _logical[LogicalOperator.LogicalAnd] = new OperatorInfo("&&", 6);
            _logical[LogicalOperator.LogicalOr] = new OperatorInfo("||", 5);

            Assignment = new OperatorInfo("=", 3);
            Block = new OperatorInfo("{}", 0);
        }

        private OperatorInfo(string token, int precedence)
        {
            Token = token;
            Precedence = precedence;
        }

        public static OperatorInfo Assignment { get; set; }

        public static OperatorInfo Block { get; set; }

        public static OperatorInfo GetUnary(UnaryOperator unary, bool prefix)
        {
            var dict = prefix ? _unaryprefix : _unarypostfix;
            return dict[unary];
        }

        internal static OperatorInfo GetBinary(BinaryOperator binary)
        {
            if(!_binary.ContainsKey(binary))
            {
                throw new Exception("[W2H001] operator '" + binary + "' is not supported.");
            }

            return _binary[binary];
        }

        internal static OperatorInfo GetLogical(LogicalOperator logical)
        {
            return _logical[logical];
        }
    }
}
