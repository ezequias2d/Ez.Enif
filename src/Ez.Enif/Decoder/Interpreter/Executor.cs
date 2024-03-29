﻿using System;
using System.Threading;

namespace Ez.Enif.Decoder.Interpreter
{
    internal class Executor : Stmt.IVisitor<object>
    {
        private readonly EnifManager _context;
        private readonly ExpressionExecutor _expressionExecutor;
        private CancellationToken _cancelationToken;
        public Executor(EnifManager context)
        {
            _context = context;
            _expressionExecutor = new ExpressionExecutor();
        }

        public Session Root { get; private set; }

        public void Execute(ReadOnlySpan<Stmt> statements, CancellationToken cancellationToken)
        {
            _cancelationToken = cancellationToken;
            Root = new Session();
            _expressionExecutor.SetCurrentSession(Root);
            try
            {
                foreach (var statement in statements)
                    Execute(statement);
            }
            catch(RuntimeException e)
            {
                _context.RuntimeError(e);
            }
        }

        private object Execute(Stmt statement)
        {
            _cancelationToken.ThrowIfCancellationRequested();
            return statement.Accept(this);
        }

        public object Session(Stmt.Session stmt)
        {
            _expressionExecutor.SetCurrentSession(Root.CreateSessionPath(stmt.Name.Literal.ToString()));
            return null;
        }

        public object Expression(Stmt.Expression stmt)
        {
            if (stmt.Expr != null)
                _expressionExecutor.Evaluate(stmt.Expr);
            return null;
        }
    }
}
