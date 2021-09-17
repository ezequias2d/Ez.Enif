using System.Collections;

namespace Ez.Enif.Serialization
{
    internal static class EnumeratorHelpers
    {
        public static bool IsSingle(this IEnumerable values)
        {
            var enumerator = values.GetEnumerator();
            return enumerator.MoveNext() && !enumerator.MoveNext();
        }

        public static bool IsEmpty(this IEnumerable values)
        {
            var enumerator = values.GetEnumerator();
            return !enumerator.MoveNext();
        }

        public static object First(this IEnumerable values)
        {
            var enumerator = values.GetEnumerator();
            if (enumerator.MoveNext())
                return enumerator.Current;
            return null;
        }
    }
}
