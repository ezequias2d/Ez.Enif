using System;
using System.Globalization;

namespace Ez.Enif
{
    internal static class EscapeParser
    {

        public static int Parse(ReadOnlySpan<char> values, out string result)
        {
            result = null;
            int readed = 1;
            switch (values[0])
            {
                case 'a':
                    result = "\a";
                    break;
                case 'b':
                    result = "\b";
                    break;
                case 'e':
                    result = "\x1B";
                    break;
                case 'f':
                    result = "\f";
                    break;
                case 'n':
                    result = "\n";
                    break;
                case 'r':
                    result = "\r";
                    break;
                case 't':
                    result = "\t";
                    break;
                case 'v':
                    result = "\x0B";
                    break;
                case '\\':
                    result = "\\";
                    break;
                case '\'':
                    result = "\'";
                    break;
                case '"':
                    result = "\"";
                    break;
                case '?':
                    result = "?"; 
                    break;
                case 'x':
                    result = DecodeHex(values[1..], out readed);
                    break;
                case 'u':
                    result = DecodeUnicode16(values[1..], out readed);
                    break;
                case 'U':
                    result = DecodeUnicode32(values[1..], out readed);
                    break;
                default:
                    TryDecodeOctal(values, out result, out readed);
                    break;
            }
            return readed;
        }

        private static bool TryDecodeOctal(ReadOnlySpan<char> values, out string result, out int readed)
        {
            result = null;
            readed = 0;

            values = values.Slice(0, Math.Min(3, values.Length));
            
            // check valid octal
            foreach (var value in values)
                if (!char.IsDigit(value))
                    return false;

            readed = values.Length;
            result = ((char)Convert.ToInt16(values.ToString(), 8)).ToString();
            return true;
        }

        private static string DecodeHex(ReadOnlySpan<char> values, out int readed)
        {
            readed = Math.Min(2, values.Length);
            var str = values.Slice(0, readed).ToString();
            return ((char)Convert.ToByte(str, 16)).ToString();
        }

        private static string DecodeUnicode16(ReadOnlySpan<char> values, out int readed)
        {
            readed = Math.Min(4, values.Length);
            var str = values.Slice(0, readed).ToString();
            return ((char)Convert.ToInt16(str, 16)).ToString();
        }

        private static string DecodeUnicode32(ReadOnlySpan<char> values, out int readed)
        {
            readed = Math.Min(8, values.Length);
            values = values.Slice(0, readed);
            var utf32 = int.Parse(values, NumberStyles.HexNumber);
            return char.ConvertFromUtf32(utf32);
        }
    }
}
