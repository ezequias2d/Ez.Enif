using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;

namespace Ez.Enif.Serialization
{
    public class EnifTypeConverter : EnifConverter
    {
        private readonly IDictionary<Type, TypeConverter> _converters;
        private readonly CultureInfo _cultureInfo;
        public EnifTypeConverter()
        {
            _converters = new Dictionary<Type, TypeConverter>();
            _cultureInfo = CultureInfo.InvariantCulture;
        }

        public override bool CanConvert(Session session, string name, object value) =>
            CanConvert(session, name, value.GetType());

        public override bool CanConvert(Session session, string name, Type typeToConvert) =>
            typeToConvert.GetCustomAttributes(typeof(TypeConverterAttribute), true).Any();

        public override IEnumerable Read(EnifReader reader, Session current, string name, Type sourceType)
        {
            var converter = GetTypeConverter(sourceType);
            var strValues = reader.Read(current, name, typeof(string));

            foreach (var strValue in strValues)
                yield return converter.ConvertFromString(null, CultureInfo.InvariantCulture, (string)strValue);
        }

        public override void Write(EnifWriter writer, Session current, string name, IEnumerable values)
        {
            var converter = GetTypeConverter(values.First().GetType());
            var strValues = new List<string>();
            foreach(var value in values)
                strValues.Add((string)converter.ConvertTo(null, _cultureInfo, value, typeof(string)));
            writer.Write(current, name, strValues);
        }

        private TypeConverter GetTypeConverter(Type type)
        {
            if (_converters.TryGetValue(type, out var value))
                return value;

            var converterAttribute = type.GetCustomAttributes(typeof(TypeConverterAttribute), true).First() as TypeConverterAttribute;

            var converterType = Type.GetType(converterAttribute.ConverterTypeName);
            var constructor = converterType.GetConstructor(Array.Empty<Type>());

            if (constructor == null)
                throw new Exception($"No parameterless constructor for {converterType.Name}.");

            value = constructor.Invoke(Array.Empty<object>()) as TypeConverter;
            _converters.Add(type, value);
            return value;
        }
    }
}