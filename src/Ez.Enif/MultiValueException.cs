using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Enif
{
    public class MultiValueException : EnifException
    {
        public MultiValueException()
        {
        }

        public MultiValueException(string message) : base(message)
        {
        }

        public MultiValueException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected MultiValueException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
