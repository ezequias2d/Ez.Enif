using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Ez.Enif.Serialization
{
    public sealed class EnifReader
    {
        public EnifReader(Session root)
        {
            Root = root;
            Version = GetType().Assembly.GetName().Version;
            Converters = new List<EnifConverter>(new EnifConverter[]
            {
                new PrimitiveConverter(),
                new ReferenceConverter(),
                new ArrayConverter(),
                new EnifTypeConverter(),
                new ValueConverter(),
            });
        }

        public Session Root { get; }
        public Version Version { get; }
        public ICollection<EnifConverter> Converters { get; }

        public IEnumerable Read(Session session, string name, Type type)
        {
            foreach (var converter in Converters)
            {
                if (converter.CanConvert(session, name, type))
                {
                    var result = converter.Read(this, session, name, type);
                    if (result != null)
                        return result;
                }
            }

            return Array.Empty<object>();
        }

        public object SingleRead(Session session, string name, Type type) =>
            Read(session, name, type).First();
    }
}
