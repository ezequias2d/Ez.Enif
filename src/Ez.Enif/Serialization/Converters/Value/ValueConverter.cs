using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Linq;
using System.Collections;

namespace Ez.Enif.Serialization
{
    /// <summary>
    /// Converts a object in a value version of that.
    /// </summary>
    public class ValueConverter : EnifConverter
    {
        private readonly IDictionary<Type, EnifType> _types;

        public ValueConverter()
        {
            _types = new Dictionary<Type, EnifType>();
        }

        internal EnifType GetEnifType(Type type)
        {
            if (_types.TryGetValue(type, out var value))
                return value;
            value = new EnifType(type);
            _types[type] = value;
            return value;
        }

        public override bool CanConvert(Session session, string name, object value) => CanConvert(session, name, value.GetType());

        public override bool CanConvert(Session session, string name, Type typeToConvert)
        {
            var enifType = GetEnifType(typeToConvert);
            if (enifType.IsPrimitiveOrString)
                return false;
            return true;
        }

        public override IEnumerable Read(EnifReader reader, Session current, string name, Type sourceType)
        {
            var subsession = GetSubsession(current, name);
            
            if(!TryMultiRead(reader, subsession, sourceType, out var values))
                values = new[] { SingleRead(reader, subsession, sourceType) };

            return values;
        }

        private object SingleRead(EnifReader reader, Session current, Type sourceType)
        {
            var type = GetEnifType(sourceType);
            object result;

            if (type.IsReference)
                result = type.Create();
            else
                result = FormatterServices.GetUninitializedObject(sourceType);

            foreach (var propertyInfo in type.Properties)
            {
                var value = reader.SingleRead(current, propertyInfo.ElementName, propertyInfo.PropertyType);
                propertyInfo.SetValue(result, value);
            }

            return result;
        }

        private static bool TryMultiRead(EnifReader reader, Session current, Type sourceType, out object[] values)
        {
            var mutlivalue = ReadMultivalueFlag(reader, current);
            values = null;
            if (!mutlivalue)
                return false;

            var length = (int)reader.SingleRead(current, "#length", typeof(int));
            values = new object[length];

            for(var i = 0; i < length; i++)
                values[i] = reader.SingleRead(current, i.ToString(), sourceType);

            return true;
        }

        public override void Write(EnifWriter writer, Session current, string name, IEnumerable values)
        {
            var subsession = CreateSubsession(current, name);

            if (values.IsEmpty())
            {
            }
            else if(values.IsSingle())
            {
                SingleWrite(writer, subsession, values.First());
            }
            else
            {
                WriteMultivalueFlag(writer, current, true);
                MultiWrite(writer, subsession, values);
            }
        }

        private void SingleWrite(EnifWriter writer, Session current, object value)
        {
            var type = GetEnifType(value.GetType());
            var temp = new IConvertible[1];
            ref var temp0 = ref temp[0];
            foreach (var propertyInfo in type.Properties)
            {
                var propertyValue = propertyInfo.GetValue(value);

                writer.Write(current, propertyInfo.ElementName, propertyValue);
            }
        }

        private static void MultiWrite(EnifWriter writer, Session current, IEnumerable values)
        {
            var count = 0;
            foreach(var value in values)
            {
                writer.Write(current, count++.ToString(), value);
                count++;
            }

            writer.Write(current, "#length", count);
        }

        private static Session CreateSubsession(Session current, string name)
        {
            if (!current.TryCreate(name, out var subsession))
                throw new Exception($"Can not create {name} subsession!");
            return subsession;
        }

        private static Session GetSubsession(Session current, string name)
        {
            if(!current.TryGetValue(name, out var subsession))
                throw new Exception($"Can not localize {name} session!");
            return subsession;
        }

        private static void WriteMultivalueFlag(EnifWriter writer, Session current, bool value)
        {
            writer.Write(current, "#multivalue", value);
        }

        private static bool ReadMultivalueFlag(EnifReader reader, Session current)
        {
            var value = reader.SingleRead(current, "#multivalue", typeof(bool));
            if (value == null)
                return false;
            return (bool)value;
        }
    }
}
