using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace Ez.Enif.Serialization
{
    internal class PreserveReferenceResolver
    {
        private readonly ObjectIDGenerator _idGenerator;
        private readonly IDictionary<string, object> _refToValue;
        private readonly IDictionary<object, string> _valueToRef;

        public PreserveReferenceResolver()
        {
            _valueToRef = new Dictionary<object, string>(ReferenceEqualityComparer.Instance);
            _idGenerator = new();
            _refToValue = new Dictionary<string, object>();
            Suffix = '@';
        }

        public readonly char Suffix;

        public void AddReference(string reference, object value)
        {
            Debug.Assert(_refToValue != null);

            var addRefToValue = !_refToValue.TryAdd(reference, value);
            var addValueToRef = !_valueToRef.TryAdd(value, reference);

            if (addRefToValue | addValueToRef)
                throw new InvalidOperationException();

        }

        public string GetReference(object value, out bool alreadyExists)
        {
            Debug.Assert(_valueToRef != null);
            Debug.Assert(_idGenerator != null);

            alreadyExists = true;
            if(!_valueToRef.TryGetValue(value, out var reference))
            {
                reference = $"{Suffix}{_idGenerator.GetId(value, out _)}";
                alreadyExists =  false;
            }

            return reference;
        }

        public object ResolveReference(string reference)
        {
            Debug.Assert(_refToValue != null);

            if (!_refToValue.TryGetValue(reference, out var value))
                return null;

            return value;
        }
    }
}
