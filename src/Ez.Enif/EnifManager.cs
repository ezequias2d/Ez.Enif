using Ez.Enif.Decoder.Interpreter;
using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Ez.Enif
{
    public class EnifManager
    {
        private bool _hadError;
        public EnifManager()
        {
            _hadError = false;

            NumberformatInfo = CultureInfo.InvariantCulture.NumberFormat;
        }

        internal readonly NumberFormatInfo NumberformatInfo;

        public event EventHandler<ErrorEventArgs> LogError;

        public Session Read(string text)
        {
            try
            {
                var scanner = new Scanner(this, text);
                var tokens = scanner.GetTokens();

                foreach (var token in tokens.Span)
                    Console.WriteLine(token);

                var parser = new EnifParser(tokens, this);
                var statements = parser.Parse();

                Console.WriteLine("Statements:");
                foreach (var statement in statements)
                    Console.WriteLine(statement);

                var resolver = new Resolver(this);
                resolver.Resolve(statements);

                if (_hadError)
                    return null;

                var executor = new Executor(this);
                executor.Execute(statements, default);
                return executor.Root;
            }
            catch (EnifException)
            {
                throw;
            }
        }

        internal void Error(Token token, string message)
        {
            if (token.Type == TokenType.EOF)
                Report(token.Line, token.Position, " at end", message);
            else
                Report(token.Line, token.Position, " at '" + token.Lexeme.ToString() + "'", message);
        }

        public void Report(int line, int position, string where, string message)
        {
            var sb = new StringBuilder();
            sb.Append("[line ");
            sb.Append(line);
            sb.Append(", token index ");
            sb.Append(position);
            sb.Append("] Error ");
            sb.Append(where);
            sb.Append(": ");
            sb.Append(message);

            LogError?.Invoke(this, new(sb.ToString()));

            _hadError = true;
        }

        public void RuntimeError(RuntimeException e)
        {
            var sb = new StringBuilder();
            sb.Append("[line ");
            sb.Append(e.Token.Line);
            sb.Append(", token index ");
            sb.Append(e.Token.Position);
            sb.Append("] Token '");
            sb.Append(Regex.Unescape(e.Token.Lexeme.ToString()));
            sb.Append('\'');

            LogError?.Invoke(this, new(sb.ToString()));
        }
    }
}
