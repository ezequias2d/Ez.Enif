using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Ez.Enif.Serialization
{
    internal class ArrayConverter : EnifConverter
    {
        public override bool CanConvert(Session session, string name, object value) =>
            CanConvert(session, name, value.GetType());

        public override bool CanConvert(Session session, string name, Type typeToConvert) =>
            typeToConvert.IsAssignableTo(typeof(Array));

        public override IEnumerable Read(EnifReader reader, Session current, string name, Type sourceType)
        {
            if(TrySingleRead(reader, current, name, sourceType, out var result))
            {
                return Enumerable.Repeat(result, 1);
            }
            return MultiRead(reader, current, name, sourceType);
        }

        private static bool TrySingleRead(EnifReader reader, Session current, string name, Type sourceType, out Array result)
        {
            var elementType = sourceType.GetElementType();
            result = null;
            if (elementType == typeof(byte))
            {
                if (!current.Properties.TryGetValue(name, out var property))
                    return false;

                result = Convert.FromBase64String(property[0].ToString(CultureInfo.InvariantCulture));
                return true;
            }
            else if (elementType.IsPrimitive || elementType.IsAssignableTo(typeof(string)))
            {
                if (!current.Properties.TryGetValue(name, out var property))
                    return false;

                var array = Array.CreateInstance(elementType, property.Count);
                for (var i = 0; i < property.Count; i++)
                    array.SetValue(property[i].ToType(elementType, CultureInfo.InvariantCulture), i);
                result = array;
                return true;
            }
            else if (current.TryGetValue(name, out var subsession))
            {
                var values = reader.Read(subsession, "values", elementType);

                if (values.IsEmpty())
                    return false;

                var length = (int)reader.SingleRead(subsession, "length", typeof(int));
                var array = Array.CreateInstance(elementType, length);

                var i = 0;
                foreach(var value in values)
                    array.SetValue(value, i++);

                result = array;
                return true;
            }
            return false;
        }

        private static IEnumerable MultiRead(EnifReader reader, Session current, string name, Type sourceType)
        {
            if (!current.TryGetValue(name, out var subsession))
                throw new Exception();

            var length = (int)reader.SingleRead(subsession, "length", typeof(int));

            for (var i = 0; i < length; i++)
                yield return reader.Read(subsession, i.ToString(), sourceType);
        }

        public override void Write(EnifWriter writer, Session current, string name, IEnumerable values)
        {
            if (values.IsEmpty())
            {
                current.Properties.Add(name, new Property());
            }
            else if (values.IsSingle())
            {
                SingleWrite(writer, current, name, (Array)values.First());
            }
            else
            {
                MultiWrite(writer, current, name, values.Cast<Array>());
            }
        }

        private static void SingleWrite(EnifWriter writer, Session current, string name, Array array)
        {
            var type = array.GetType();
            var elementType = type.GetElementType();

            if (elementType == typeof(byte))
            {
                var base64 = Convert.ToBase64String((byte[])array, Base64FormattingOptions.None);
                current.Properties.Add(name, new(new[] { base64 }));
                return;
            }
            else if (elementType.IsPrimitive || elementType.IsAssignableTo(typeof(string)))
            {
                current.Properties.Add(name, new(array.Cast<IConvertible>()));
            }
            else if (current.TryCreate(name, out var subsession))
            {
                writer.Write(subsession, "length", array.Length);
                writer.Write(subsession, "values", (IEnumerable<object>)array);
            }
            else
                throw new Exception();
        }

        private static void MultiWrite(EnifWriter writer, Session current, string name, IEnumerable<Array> arrays)
        {
            if (!current.TryCreate(name, out var subsession))
                throw new Exception();

            var count = 0;
            foreach(Array array in arrays)
                writer.Write(subsession, count++.ToString(), array);

            writer.Write(subsession, "length", count);
        }
    }
}
