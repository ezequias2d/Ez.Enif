using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Ez.Enif
{
    public class Property : IList<IConvertible>
    {
        private readonly IList<IConvertible> _values;

        public Property() : this(Enumerable.Empty<IConvertible>())
        {
        }

        public Property(IEnumerable<IConvertible> values)
        {
            _values = new List<IConvertible>(values);
        }

        public IConvertible this[int index] { get => _values[index]; set => _values[index] = value; }

        public int Count => _values.Count;

        public bool IsReadOnly => false;

        public void Add(IConvertible item) => _values.Add(item);

        public void Clear() => _values.Clear();

        public bool Contains(IConvertible item) => _values.Contains(item);

        public void CopyTo(IConvertible[] array, int arrayIndex) => _values.CopyTo(array, arrayIndex);

        public IEnumerator<IConvertible> GetEnumerator() => _values.GetEnumerator();

        public int IndexOf(IConvertible item) => _values.IndexOf(item);

        public void Insert(int index, IConvertible item) => _values.Insert(index, item);

        public bool Remove(IConvertible item) => _values.Remove(item);

        public void RemoveAt(int index) => _values.RemoveAt(index);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString() => string.Join(' ', this.Select((a) => a.ToString(CultureInfo.InvariantCulture)));
    }
}
