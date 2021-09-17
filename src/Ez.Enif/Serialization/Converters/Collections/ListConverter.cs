using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ez.Enif.Serialization
{
    public class ListConverter : EnifConverter
    {
        public override bool CanConvert(Session session, string name, object value)
        {
            throw new NotImplementedException();
        }

        public override bool CanConvert(Session current, string name, Type type)
        {
            if (!type.IsGenericType)
                return false;

            try
            {
                var genericArguments = type.GetGenericArguments();
                return type.IsAssignableFrom(typeof(List<>).MakeGenericType(genericArguments));
            }
            catch 
            {
                return false;
            }
        }

        public override IEnumerable Read(EnifReader reader, Session current, string name, Type sourceType)
        {
            throw new NotImplementedException();
        }

        public override void Write(EnifWriter writer, Session current, string name, IEnumerable values)
        {
            throw new NotImplementedException();
        }
    }
}
