using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Ez.Enif
{
    public abstract class Expr
    {
        public abstract R Accept<R>(Visitor<R> visitor);
        public interface Visitor<R>
        {
            R AssignExpr(Assign expr);
            R BinaryExpr(Binary expr);
            R CallExpr(Call expr);
            R GetExpr(Get expr);
            R GroupingExpr(Grouping expr);
            R LiteralExpr(Literal expr);
            R LogicalExpr(Logical expr);
            R UnaryExpr(Unary expr);
            R VariableExpr(Variable expr);
        }

        public override string ToString()
        {
            var type = GetType();
            var sb = new StringBuilder();
            sb.Append(type.Name);

            var fields = type.GetFields();

            sb.Append('(');
            {
                var e = fields.GetEnumerator();
                var condition = true;
                e.MoveNext();
                while (condition)
                {
                    var field = (FieldInfo)e.Current;
                    var value = field.GetValue(this);
                    sb.Append($"{field.Name}: {value}");
                    condition = e.MoveNext();
                    if (condition)
                        sb.Append(", ");
                }
            }
            sb.Append(')');

            return sb.ToString();
        }

        public class Assign : Expr
        {
            public readonly Token Name;
            public readonly IEnumerable<Expr> Values;

            public Assign(Token name, IEnumerable<Expr> values)
            {
                Name = name;
                Values = values;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.AssignExpr(this);
            }

            public override string ToString()
            {
                var type = GetType();
                var sb = new StringBuilder();
                sb.Append(type.Name);
                sb.Append($"(Name {Name})");
                sb.Append("(Values {");
                
                {
                    var e = Values.GetEnumerator();
                    var condition = true;
                    e.MoveNext();
                    while (condition)
                    {
                        sb.Append($"{e.Current}");
                        condition = e.MoveNext();
                        if (condition)
                            sb.Append(", ");
                    }
                }
                sb.Append("})");

                return sb.ToString();
            }
        }

        public class Binary : Expr
        {
            public readonly Expr Left;
            public readonly Token Operator;
            public readonly Expr Right;

            public Binary(Expr left, Token op, Expr right)
            {
                Left = left;
                Operator = op;
                Right = right;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.BinaryExpr(this);
            }
        }

        public class Call : Expr
        {
            public readonly Expr Callee;
            public readonly Token Paren;
            public readonly Expr[] Arguments;

            public Call(Expr callee, Token paren, Expr[] arguments)
            {
                Callee = callee;
                Paren = paren;
                Arguments = arguments;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.CallExpr(this);
            }
        }

        public class Get : Expr
        {
            public readonly Expr Object;
            public readonly Token Name;

            public Get(Expr obj, Token name)
            {
                Object = obj;
                Name = name;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.GetExpr(this);
            }
        }

        public class Grouping : Expr
        {
            public readonly Expr Expression;

            public Grouping(Expr expression)
            {
                Expression = expression;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.GroupingExpr(this);
            }
        }

        public class Literal : Expr
        {
            public readonly object Value;

            public Literal(object value)
            {
                Value = value;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.LiteralExpr(this);
            }
        }

        public class Logical : Expr
        {
            public readonly Expr Left;
            public readonly Token Operator;
            public readonly Expr Right;

            public Logical(Expr left, Token op, Expr right)
            {
                Left = left;
                Operator = op;
                Right = right;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.LogicalExpr(this);
            }
        }

        public class Unary : Expr
        {
            public readonly Token Operator;
            public readonly Expr Right;

            public Unary(Token op, Expr right)
            {
                Operator = op;
                Right = right;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.UnaryExpr(this);
            }
        }

        public class Variable : Expr
        {
            public Token Name;
            public Variable(Token name)
            {
                Name = name;
            }

            public override R Accept<R>(Visitor<R> visitor)
            {
                return visitor.VariableExpr(this);
            }
        }
    }
}
