using System;

namespace Ez.Enif
{
    public class RuntimeException : Exception
    {
        internal readonly Token Token;

        internal RuntimeException(Token token, string message) : base(message)
        {
            Token = token;
        }
    }
}
