using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Ez.Enif
{
    internal abstract class Stmt
    {
        public interface IVisitor<R>
        {
            R Expression(Expression stmt);
            R Session(Session stmt);
        }

        public abstract R Accept<R>(IVisitor<R> visitor);

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

        public class Session : Stmt
        {
            public readonly Token Name;
            public Session(Token name)
            {
                Name = name;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.Session(this);
            }
        }

        public class Expression : Stmt
        {
            public readonly Expr Expr;

            public Expression(Expr expr)
            {
                Expr = expr;
            }

            public override R Accept<R>(IVisitor<R> visitor)
            {
                return visitor.Expression(this);
            }
        }
    }
}
