using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Ez.Enif
{
    internal class Scanner
    {
        static Scanner()
        {
        }

        private readonly EnifManager _context;
        private readonly ReadOnlyMemory<char> _source;
        private readonly int _length;
        private int _start;
        private int _current;
        private int _line;
        private int _position;
        private readonly HashSet<char> _reserverds;
        private readonly HashSet<char> _quotes;
        private readonly HashSet<char> _ignores;

        public Scanner(EnifManager context, string source)
        {
            _context = context;
            _source = new Memory<char>(source.ToCharArray());
            _length = _source.Length;
            _reserverds = new HashSet<char>(new char[] { '[', ']', '=', ';', '"', '\'', ' ', '+', '-', '/', '*' });
            _quotes = new HashSet<char>(new char[] { '\'', '"' });
            _ignores = new HashSet<char>(new char[] { '\n', '\r', '\t', ' ' });
        }

        public ReadOnlyMemory<Token> GetTokens()
        {
            var source = _source.Span;
            var tokens = new List<Token>();
            _start = 0;
            _current = 0;
            _line = 1;
            _position = 1;

            Token aux;
            while (_current < _length)
            {
                _start = _current;

                aux = GetToken(source);
                _position++;
                if (aux.Type != TokenType.Trash)
                {
                    tokens.Add(aux);
                }
            }
            tokens.Add(new Token(TokenType.EOF, new Memory<char>(string.Empty.ToCharArray()), null, _line, _position));
            return tokens.ToArray();
        }

        private Token GetToken(ReadOnlySpan<char> source)
        {
            switch (source[_current++])
            {
                case '[':
                    return MakeToken(TokenType.LeftBracket);
                case ']':
                    return MakeToken(TokenType.RightBracket);
                case '=':
                    return MakeToken(TokenType.Equal);
                case ';':
                    while (_current < _length && source[_current] != '\n')
                        ++_current;
                    return MakeToken(TokenType.Trash);
                #region math
                case '+':
                    return MakeToken(TokenType.Plus);
                case '-':
                    return MakeToken(TokenType.Minus);
                case '/':
                    return MakeToken(TokenType.Slash);
                case '*':
                    return MakeToken(TokenType.Star);
                case '&':
                    return MakeToken(TokenType.And);
                case '|':
                    return MakeToken(TokenType.Or);
                case '\n':
                    return MakeToken(TokenType.NewLine);
                #endregion
                default:
                    char c = source[_current - 1];
                    if (_ignores.Contains(c))
                    {
                        if (c == '\n')
                            _line++;
                        return MakeToken(TokenType.Trash);
                    }

                    if (char.IsDigit(c) && TryMakeNumberToken(source, out var token))
                        return token;
                    else
                        return MakeStringOrIdentifierToken(source);
            }
        }

        private Token MakeToken(TokenType type, object literal = null)
        {
            return new Token(type, _source[_start.._current], literal, _line, _position);
        }

        private Token MakeStringOrIdentifierToken(ReadOnlySpan<char> source)
        {
            var sb = new StringBuilder();

            var hasQuotes = false;
            var endChar = '\n';

            if (_quotes.Contains(source[_start]))
            {
                endChar = source[_start];
                hasQuotes = true;

                // jump first character(the quote)
                _current = _start + 1;
            }
            else
                // read by the first character
                _current = _start;

            if (!hasQuotes)
            {
                // advance to first non-whitespace character
                // NOTE: do not enable with quotes, because quotes are more literal about what there is
                while (_current < _length && source[_current] == ' ')
                    _current++;
            }

            bool lineBreak = false;

            // read character by character up to a reserved character
            
            while (_current < _length && (!_reserverds.Contains(source[_current]) || (hasQuotes && source[_current] == ' ')))
            {
                var temp = source[_current];
                switch (temp)
                {
                    // ends in a new line
                    case '\n':
                        ++_line;
                        // ends test read if has new line without linebreak
                        if (!lineBreak)
                            goto End;
                        else
                            // disable linebreak(each new line needs a linebreak to keep breaking the lines)
                            lineBreak = false;
                        break;
                    case '\\':
                        // increment _current to read next char
                        var readed = EscapeParser.Parse(source[(_current + 1)..], out var result);
                        if(readed > 0)
                        {
                            // write if parse
                            _current += readed;
                            sb.Append(result);
                        }
                        else
                        {
                            // enable linebreak
                            lineBreak = true;
                        }
                        break;
                    case ' ':
                        // only add spaces if is quoted string and is not in linebreak area(before linebreak and \n char)
                        if (hasQuotes && !lineBreak)
                            sb.Append(' ');
                        break;
                    default:
                        if (lineBreak)
                            return MakeToken(TokenType.Error, "LineBreak error! linebreaks only work when is last character(disregarding whitespace and newline)");

                        if(!_ignores.Contains(temp))
                            sb.Append(temp);
                        break;
                }

                ++_current;
            }
            End:

            if (endChar != '\n' && _current < _length && source[_current] != endChar)
                return MakeToken(TokenType.Error, $"The string must be {Regex.Escape(endChar.ToString())} terminated.");

            if (hasQuotes)
            {
                if (_current >= _length)
                    return MakeToken(TokenType.Error, "Unterminated string.");

                if (_current < _length && source[_current] != endChar)
                    return MakeToken(TokenType.Error, $"Invalid termination string quotes! Expected: {endChar}, Value: {source[_current]}");
            }

            if (hasQuotes)
            {
                // increment count to remove quotes from stream
                ++_current;
            }

            var token = TokenType.String;
            if (!hasQuotes)
                token = TokenType.Identifier;

            return new Token(token, _source[_start.._current], sb.ToString(), _line, _position);
        }

        private bool TryMakeNumberToken(ReadOnlySpan<char> source, out Token token)
        {
            token = default;
            while (_current < _length && char.IsDigit(source[_current]))
                ++_current;

            if (_current < _length)
            {

                if (source[_current] == '.')
                {
                    var next = _current + 1;
                    var nextChar = source[next];
                    // invalid number format!
                    if (next >= _length || (!char.IsDigit(nextChar) && !_reserverds.Contains(nextChar)))
                        return false;

                    _current = next;

                    while (_current < _length && char.IsDigit(source[_current]))
                        ++_current;

                    if(_current < _length)
                    {
                        var currentChar = source[_current];
                        if (!char.IsDigit(currentChar) && !_reserverds.Contains(currentChar))
                            return false;
                    }
                }
                else if (char.IsLetter(source[_current]))
                    return false;
            }

            string value = source[_start.._current].ToString();

            token = MakeToken(TokenType.Number, double.Parse(value, _context.NumberformatInfo));
            return true;
        }
    }
}
