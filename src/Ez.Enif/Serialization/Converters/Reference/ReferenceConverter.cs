using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace Ez.Enif.Serialization
{
    internal class ReferenceConverter : EnifConverter
    {
        private readonly PreserveReferenceResolver _referenceResolver;

        public ReferenceConverter()
        {
            _referenceResolver = new();
        }

        public override bool CanConvert(Session session, string name, object value) 
        {
            Debug.Assert(value != null);

            if (!CanConvert(session, name, value.GetType()))
                return false;

            _referenceResolver.GetReference(value, out var alreadyExists);
            return !alreadyExists;
        }

        public override bool CanConvert(Session session, string name, Type typeToConvert) =>
            !(typeToConvert.IsValueType || typeToConvert.IsAssignableTo(typeof(string))) && !name.StartsWith(_referenceResolver.Suffix);

        public override IEnumerable Read(EnifReader reader, Session current, string name, Type sourceType)
        {
            if (!current.Properties.TryGetValue(name, out var property))
                return Array.Empty<object>();

            var results = new List<object>();

            var value = property.GetEnumerator();

            while (value.MoveNext())
            {
                var reference = value.Current.ToString(CultureInfo.InvariantCulture);
                var result = _referenceResolver.ResolveReference(reference);

                if (result == null)
                    result = reader.SingleRead(reader.Root, reference, sourceType);

                _referenceResolver.AddReference(reference, result);
                results.Add(result);
            }

            return results;
        }

        public override void Write(EnifWriter writer, Session current, string name, IEnumerable values)
        {
            var references = new List<string>();

            foreach(var value in values)
            {
                var reference = _referenceResolver.GetReference(value, out var alreadyExist);
                references.Add(reference);

                if (!alreadyExist)
                {
                    writer.Write(reference, value);
                    _referenceResolver.AddReference(reference, value);
                }
            }

            writer.Write(current, name, references);
        }
    }
}
