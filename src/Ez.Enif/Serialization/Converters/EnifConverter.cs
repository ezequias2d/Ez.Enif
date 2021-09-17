using System;
using System.Collections;
using System.Collections.Generic;

namespace Ez.Enif.Serialization
{
    public abstract class EnifConverter
    {
        public bool SupportMultiValue { get; }
        /// <summary>
        /// When overridden in a derived class, determines whether the converter
        /// instance can convert the specified object.
        /// </summary>
        /// <param name="valueToConvert">The value to check whether it 
        /// can be converted by this converter instance.</param>
        /// <returns><see langword="true"/> if the instance can convert the specified
        /// object; otherwise, <see langword="false"/>.</returns>
        public abstract bool CanConvert(Session session, string name, object value);
        public abstract bool CanConvert(Session current, string name, Type type);
        public abstract IEnumerable Read(EnifReader reader, Session current, string name, Type sourceType);
        public abstract void Write(EnifWriter writer, Session current, string name, IEnumerable values);
    }
}
