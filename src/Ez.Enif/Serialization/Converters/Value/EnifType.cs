using System;
using System.Collections.Generic;
using System.Reflection;

namespace Ez.Enif.Serialization
{
    internal class EnifType
    {
        private readonly ConstructorInfo _constructor;
        public readonly bool IsReference;
        public readonly bool IsPrimitiveOrString;
        public readonly ICollection<EnifPropertyInfo> Properties;

        public EnifType(Type type)
        {
            IsPrimitiveOrString = type.IsPrimitive || type.IsAssignableTo(typeof(string));
            IsReference = !type.IsValueType;

            Properties = ScanProperties(type);
            _constructor = GetDefaultConstructor(type);
        }

        public object Create() => _constructor.Invoke(Array.Empty<object>());

        private static ICollection<EnifPropertyInfo> ScanProperties(Type type)
        {
            var properties = new List<EnifPropertyInfo>();
            foreach (var property in type.GetProperties())
            {
                if(EnifPropertyInfo.TryCreate(property, out var pi))
                {
                    properties.Add(pi);
                }
            }

            foreach (var field in type.GetFields())
            {
                if (EnifPropertyInfo.TryCreate(field, out var pi))
                    properties.Add(pi);
            }

            return properties;
        }

        private static ConstructorInfo GetDefaultConstructor(Type type) 
        {
            ConstructorInfo constructor;

            constructor = type.GetConstructor(Array.Empty<Type>());

            return constructor;
        } 
    }
}
