using System;
using System.Collections.Generic;

namespace Ez.Enif.Decoder.Interpreter
{
    internal static class Operator
    {
        public static bool Operation(IConvertible first, IConvertible second, TokenType operationToken, IFormatProvider provider, out IConvertible result)
        {
            var firstMult = first as MultiValueConvertible;
            var secondMult = second as MultiValueConvertible;

            if (firstMult != null || secondMult != null)
            {
                if (firstMult == null)
                    firstMult = new MultiValueConvertible(new[] { first });
                else if (secondMult == null)
                    secondMult = new MultiValueConvertible(new[] { second });
                if (!MultOperation(firstMult, secondMult, operationToken, null, out result))
                    return false;
                return true;
            }

            return SimpleOperation(first, second, operationToken, provider, out result);
        }

        public static bool Negate(IConvertible first, IFormatProvider provider, out IConvertible result)
        {
            var mult = first as MultiValueConvertible;

            if (first != null)
                return MultNegate(mult, provider, out result);
            return SimpleNegate(first, provider, out result);
        }

        private static bool MultNegate(MultiValueConvertible first, IFormatProvider provider, out IConvertible result)
        {
            var list = new List<IConvertible>();
            result = default;
            foreach (var value in first.Values)
                if (SimpleNegate(value, provider, out var temp))
                    list.Add(temp);
                else
                    return false;
            return true;
        }

        private static bool SimpleNegate(IConvertible first, IFormatProvider provider, out IConvertible result)
        {
            var code1 = first.GetTypeCode();
            result = code1 switch
            {
                TypeCode.Double => first.ToDouble(provider),
                _ => first.ToInt64(provider),
            };
            return true;
        }

        private static bool MultOperation(MultiValueConvertible first, MultiValueConvertible second, TokenType operationToken, IFormatProvider provider, out IConvertible result)
        {
            var firstEnumerator = first.Values.GetEnumerator();
            var secondEnumerator = second.Values.GetEnumerator();
            var list = new List<IConvertible>();

            bool moveNext;
            do
            {
                var firstMoveNext = firstEnumerator.MoveNext();
                var secondMoveNext = secondEnumerator.MoveNext();
                moveNext = firstMoveNext || secondMoveNext;

                if (firstMoveNext && secondMoveNext)
                {
                    if (!SimpleOperation(firstEnumerator.Current, secondEnumerator.Current, operationToken, provider, out var temp))
                    {
                        result = null;
                        return false;
                    }
                    list.Add(temp);
                }
                else if (firstMoveNext)
                    list.Add(firstEnumerator.Current);
                else if (secondMoveNext)
                    list.Add(secondEnumerator.Current);

            } while (moveNext);

            result = new MultiValueConvertible(list);
            return true;
        }

        private static bool SimpleOperation(IConvertible first, IConvertible second, TokenType operationToken, IFormatProvider provider, out IConvertible result)
        {
            var code1 = first.GetTypeCode();
            var code2 = second.GetTypeCode();

            var anyString = code1 == TypeCode.String || code2 == TypeCode.String;
            var anyDouble = code1 == TypeCode.Double || code2 == TypeCode.Double;
            result = default;
            switch (operationToken)
            {
                case TokenType.Plus:
                    if (anyString)
                        result = first.ToString(provider) + second.ToString(provider);
                    else if (anyDouble)
                        result = first.ToDouble(provider) + second.ToDouble(provider);
                    else
                        result = first.ToInt64(provider) + second.ToInt64(provider);
                    return true;
                case TokenType.Minus:
                    if (anyString)
                        return false;
                    else if (anyDouble)
                        result = first.ToDouble(provider) - second.ToDouble(provider);
                    else
                        result = first.ToInt64(provider) - second.ToInt64(provider);
                    return true;
                case TokenType.Star:
                    if (anyString)
                        return false;
                    else if (anyDouble)
                        result = first.ToDouble(provider) * second.ToDouble(provider);
                    else
                        result = first.ToInt64(provider) * second.ToInt64(provider);
                    return true;
                case TokenType.Slash:
                    if (anyString)
                        return false;
                    else if (anyDouble)
                        result = first.ToDouble(provider) / second.ToDouble(provider);
                    else
                    {
                        var value1 = first.ToInt64(provider);
                        var value2 = second.ToInt64(provider);
                        if (value1 % value2 != 0)
                            result = (double)value1 / value2;
                        else
                            result = value1 / value2;
                    }
                    return true;
                case TokenType.Greater:
                    if (!anyString)
                    {
                        if (anyString)
                            return false;
                        else if (anyDouble)
                            result = first.ToDouble(provider) > second.ToDouble(provider);
                        else
                            result = first.ToInt64(provider) > second.ToInt64(provider);
                        return true;
                    }
                    return false;
                case TokenType.GreaterEqual:
                    if (!anyString)
                    {
                        if (anyString)
                            return false;
                        else if (anyDouble)
                            result = first.ToDouble(provider) >= second.ToDouble(provider);
                        else
                            result = first.ToInt64(provider) >= second.ToInt64(provider);
                        return true;
                    }
                    return false;
                case TokenType.Less:
                    if (!anyString)
                    {
                        if (anyString)
                            return false;
                        else if (anyDouble)
                            result = first.ToDouble(provider) < second.ToDouble(provider);
                        else
                            result = first.ToInt64(provider) < second.ToInt64(provider);
                        return true;
                    }
                    return false;
                case TokenType.LessEqual:
                    if (!anyString)
                    {
                        if (anyString)
                            return false;
                        else if (anyDouble)
                            result = first.ToDouble(provider) <= second.ToDouble(provider);
                        else
                            result = first.ToInt64(provider) <= second.ToInt64(provider);
                        return true;
                    }
                    return false;
                default:
                    return false;
            }
        }
    }
}
