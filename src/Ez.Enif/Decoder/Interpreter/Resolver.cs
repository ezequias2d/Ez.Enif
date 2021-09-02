using System;
using System.Collections.Generic;
using System.Linq;

namespace Ez.Enif.Decoder.Interpreter
{
    internal class Resolver : Expr.Visitor<object>, Stmt.Visitor<object>
    {
        private readonly Context _context;
        private readonly Session _root;
        private Session _current;

        public Resolver(Context context)
        {
            _context = context;
            _root = new Session();
            _current = _root;
        }

        public void Resolve(ReadOnlySpan<Stmt> statements)
        {
            foreach (var stmt in statements)
                Resolve(stmt);
        }

        private void Resolve(Stmt statement)
        {
            statement.Accept(this);
        }

        private void Resolve(Expr expression)
        {
            expression.Accept(this);
        }

        private void Error(Token token, string message)
        {
            _context.Error(token, message);
        }

        private void ChangeSession(string session)
        {
            _current = _root.CreateSessionPath(session);
        }

        private void DeclareProperty(Token name, IEnumerable<Expr> exprs)
        {
            string propertyName = name.Literal.ToString();

            var property = new Property();
            foreach(var _ in exprs)
                property.Add(0);

            _current.Properties.Add(propertyName, property);
        }

        #region stmt
        public object Expression(Stmt.Expression stmt)
        {
            Resolve(stmt.Expr);
            return null;
        }

        public object Session(Stmt.Session stmt)
        {
            ChangeSession(stmt.Name.Literal.ToString());
            return null;
        }
        #endregion
        #region expr
        public object AssignExpr(Expr.Assign expr)
        {
            DeclareProperty(expr.Name, expr.Values);
            foreach(var value in expr.Values)
                Resolve(value);
            return null;
        }

        public object BinaryExpr(Expr.Binary expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
            return null;
        }

        public object CallExpr(Expr.Call expr)
        {
            throw new NotImplementedException();
        }

        public object GetExpr(Expr.Get expr)
        {
            throw new NotImplementedException();
        }

        public object GroupingExpr(Expr.Grouping expr)
        {
            Resolve(expr.Expression);
            return null;
        }

        public object LiteralExpr(Expr.Literal expr)
        {
            return null;
        }

        public object LogicalExpr(Expr.Logical expr)
        {
            Resolve(expr.Left);
            Resolve(expr.Right);
            return null;
        }

        public object UnaryExpr(Expr.Unary expr)
        {
            Resolve(expr.Right);
            return null;
        }

        public object VariableExpr(Expr.Variable expr)
        {
            //var name = expr.Name.Literal.ToString();
            //if (!_current.Properties.TryGetValue(name, out var values))
            //    Error(expr.Name, "The variable is not declared.");
            return null;
        }
        #endregion expr
    }
}
