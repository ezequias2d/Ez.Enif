using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Ez.Enif
{
    public struct Token
    {
        public readonly TokenType Type;
        public readonly ReadOnlyMemory<char> Lexeme;
        public readonly object Literal;
        public readonly int Line;
        public readonly int Position;

        public Token(TokenType type, ReadOnlyMemory<char> lexeme, object literal, int line, int position)
        {
            Type = type;
            Lexeme = lexeme;
            Literal = literal;
            Line = line;
            Position = position;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (Literal != null)
                sb.Append($"{Regex.Escape(Literal.ToString())}");

            sb.Append($"[{Type}]");
            return sb.ToString();
        }
    }
}
