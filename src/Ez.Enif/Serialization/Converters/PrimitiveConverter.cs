using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Ez.Enif.Serialization
{
    public class PrimitiveConverter : EnifConverter
    {
        public override bool CanConvert(Session session, string name, object value) =>
            CanConvert(session, name, value.GetType());

        public override bool CanConvert(Session current, string name, Type type) =>
            (type.IsPrimitive || type.IsAssignableTo(typeof(string)));

        public override IEnumerable Read(EnifReader reader, Session current, string name, Type sourceType)
        {
            current.Properties.TryGetValue(name, out var property);

            if (property != null)
            {
                foreach (var value in property)
                {
                    yield return value.ToType(sourceType, CultureInfo.InvariantCulture);
                }
            }
        }

        public override void Write(EnifWriter writer, Session current, string name, IEnumerable values)
        {
            current.AddValues(name, values.Cast<IConvertible>());
        }
    }
}
