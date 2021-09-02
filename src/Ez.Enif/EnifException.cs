using System;
using System.Runtime.Serialization;

namespace Ez.Enif
{
    public class EnifException : Exception
    {
        public EnifException()
        {
        }

        public EnifException(string message) : base(message)
        {
        }

        public EnifException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected EnifException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
