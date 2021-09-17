using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Enif.Decoder.Interpreter
{
    internal class ExpressionExecutor : Expr.IVisitor<IConvertible>
    {
        private Session _current;

        public void SetCurrentSession(Session current) => _current = current;

        public IConvertible Evaluate(Expr expr) => expr.Accept(this);

        public IConvertible AssignExpr(Expr.Assign expr)
        {
            var values = expr.Values.SelectMany((value) => 
            {
                var temp = Evaluate(value);
                if (temp is MultiValueConvertible mult)
                    return mult.Values;
                else
                    return new[] { temp };
            });
            var name = expr.Name.Literal.ToString();

            _current.AddValues(name, values);

            return new MultiValueConvertible(values);
        }

        public IConvertible BinaryExpr(Expr.Binary expr)
        {
            var left = Evaluate(expr.Left);
            var right = Evaluate(expr.Right);
            
            if(!Operator.Operation(left, right, expr.Operator.Type, null, out var result))
                throw new RuntimeException(expr.Operator, "Operator '" + expr.Operator + "' is poorly described.");
            
            return result;
        }

        public IConvertible CallExpr(Expr.Call expr)
        {
            throw new NotImplementedException();
        }

        public IConvertible GetExpr(Expr.Get expr)
        {
            throw new NotImplementedException();
        }

        public IConvertible GroupingExpr(Expr.Grouping expr) => Evaluate(expr.Expression);

        public IConvertible LiteralExpr(Expr.Literal expr) => (IConvertible)expr.Value;

        public IConvertible LogicalExpr(Expr.Logical expr)
        {
            var left = Evaluate(expr.Left).ToBoolean(null);

            if (expr.Operator.Type == TokenType.Or && left)
                return true;
            else if (!left) // and
                    return false;
            return Evaluate(expr.Right).ToBoolean(null);
        }

        public IConvertible UnaryExpr(Expr.Unary expr)
        {
            var right = Evaluate(expr.Right);

            switch (expr.Operator.Type)
            {
                case TokenType.Minus:
                    if (Operator.Negate(right, null, out var result))
                        return result;
                    break;
                case TokenType.Exclamation:
                    return !right.ToBoolean(null);
            }
            throw new RuntimeException(expr.Operator, "Operator '" + expr.Operator + "' is poorly described.");
        }

        public IConvertible VariableExpr(Expr.Variable expr)
        {
            var name = expr.Name.Literal.ToString();
            if (name[0] == '.' && _current.Properties.TryGetValue(name[1..], out var values))
                return new MultiValueConvertible(values);
            return name;
        }
    }
}
