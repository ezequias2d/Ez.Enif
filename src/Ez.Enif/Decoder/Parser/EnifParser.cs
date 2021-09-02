using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Ez.Enif
{
    public class EnifParser
    {
        public class ParserExpection : Exception
        {
        }

        private readonly ReadOnlyMemory<Token> _tokens;
        private readonly Context _context;
        private int current;

        public EnifParser(ReadOnlyMemory<Token> tokens, Context context)
        {
            current = 0;
            _tokens = tokens;
            _context = context;
        }

        private bool IsEOF
        {
            get
            {
                return Current.Type == TokenType.EOF;
            }
        }

        private Token Current
        {
            get
            {
                return _tokens.Span[current];
            }
        }

        private Token Previous
        {
            get
            {
                return _tokens.Span[current - 1];
            }
        }

        private bool Check(TokenType type)
        {
            return !IsEOF && Current.Type == type;
        }

        private bool Match(params TokenType[] types)
        {
            foreach (var type in types)
            {
                if (Check(type))
                {
                    current++;
                    return true;
                }
            }
            return false;
        }

        public Stmt[] Parse()
        {
            var statements = new List<Stmt>();

            while (!IsEOF)
            {
                var statement = Declaration();
                if(statement is not null)
                    statements.Add(statement);
            }

            return statements.ToArray();
        }

        private Expr Expression()
        {
            return Assignment();
        }

        private Expr Assignment()
        {
            Expr expr = Or();

            if (Match(TokenType.Equal))
            {
                Token equals = Previous;

                var values = new Queue<Expr>();

                while(!Match(TokenType.NewLine) && !IsEOF)
                    values.Enqueue(Assignment());

                if (expr is Expr.Variable variable)
                {
                    return new Expr.Assign(variable.Name, values);
                }
                Error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        private Expr Or()
        {
            Expr expr = And();

            while (Match(TokenType.Or))
            {
                Token op = Previous;
                Expr right = And();
                expr = new Expr.Logical(expr, op, right);
            }

            return expr;
        }

        private Expr And()
        {
            Expr expr = Equality();

            while (Match(TokenType.And))
            {
                Token op = Previous;
                Expr right = Equality();
                expr = new Expr.Logical(expr, op, right);
            }

            return expr;
        }

        private Expr Equality()
        {
            Expr expr = Comparison();

            while (Match(TokenType.ExclamationEqual, TokenType.EqualEqual))
            {
                Token op = Previous;
                Expr right = Comparison();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Comparison()
        {
            Expr expr = Addition();

            while (Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
            {
                Token op = Previous;
                Expr right = Addition();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Addition()
        {
            Expr expr = Multiplication();

            while (Match(TokenType.Minus, TokenType.Plus))
            {
                Token op = Previous;
                Expr right = Multiplication();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Multiplication()
        {
            Expr expr = Unary();

            while (Match(TokenType.Slash, TokenType.Star))
            {
                Token op = Previous;
                Expr right = Unary();
                expr = new Expr.Binary(expr, op, right);
            }

            return expr;
        }

        private Expr Unary()
        {
            if (Match(TokenType.Exclamation, TokenType.Minus))
            {
                Token op = Previous;
                Expr right = Unary();
                return new Expr.Unary(op, right);
            }

            return Primary();
        }

        private Expr Primary()
        {
            Expr expr = null;
            if (!IsEOF)
            {
                switch (Current.Type)
                {
                    case TokenType.False:
                        expr = new Expr.Literal(false);
                        break;
                    case TokenType.True:
                        expr = new Expr.Literal(true);
                        break;
                    case TokenType.Number:
                    case TokenType.String:
                        expr = new Expr.Literal(Current.Literal);
                        break;
                    case TokenType.LeftParent:
                        current++;
                        Expr expr2 = Expression();
                        Consume(TokenType.RightParent, "Expect ')' after expression.");
                        expr = new Expr.Grouping(expr2);
                        current--;
                        break;
                    case TokenType.Identifier:
                        expr = Variable();
                        break;
                    case TokenType.NewLine:
                        current++;
                        break;
                    default:
                        current--;
                        break;
                }
                current++;
            }
            else
            {
                throw Error(Current, "Expect expression.");
            }
            return expr;
        }

        private Expr Variable()
        {
            Expr expr;

            Token token = Current;
            expr = new Expr.Variable(token);

            return expr;
        }

        private Token Consume(TokenType type, string message)
        {
            if (Current.Type == type)
                return _tokens.Span[current++];

            throw Error(Current, message);
        }

        private Token ConsumeAny(string message, params TokenType[] types)
        {
            if (types.Contains(Current.Type))
                return _tokens.Span[current++];

            throw Error(Current, message);
        }

        private ParserExpection Error(Token token, string message)
        {
            Debug.Assert(false);
            _context.Error(token, message);
            return new ParserExpection();
        }

        private void Synchronize()
        {
            do
            {
                if (Current.Type == TokenType.NewLine)
                    return;
                ++current;
            } while (!IsEOF);
        }

        private Stmt Statement()
        {
            return ExpressionStatement();
        }

        private Stmt ExpressionStatement()
        {
            Expr expr = Expression();
            return new Stmt.Expression(expr);
        }

        private Stmt Session()
        {
            var name = ConsumeAny("Expect a identifier after first bracket.", TokenType.Identifier);

            Consume(TokenType.RightBracket, "Expect ']' after session value.");

            return new Stmt.Session(name);
        }

        private Stmt Declaration()
        {
            try
            {
                if (Match(TokenType.NewLine))
                    return null;
                if (Match(TokenType.LeftBracket))
                    return Session();
                return Statement();
            }
            catch (ParserExpection)
            {
                Synchronize();
                return null;
            }
        }
    }
}
