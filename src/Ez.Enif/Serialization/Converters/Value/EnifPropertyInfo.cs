using System;
using System.Reflection;

namespace Ez.Enif.Serialization
{
    internal struct EnifPropertyInfo
    {
        public readonly MemberInfo MemberInfo;
        public readonly string ElementName;
        public readonly bool IsNullable;
        public readonly Type PropertyType;
        private readonly Func<object, object> _get;
        private readonly Action<object, object> _set;

        private EnifPropertyInfo(MemberInfo memberInfo, string elementName, bool isNullable)
        {
            MemberInfo = memberInfo;
            ElementName = elementName;
            IsNullable = isNullable;
            
            if (memberInfo is PropertyInfo pi)
            {
                _get = pi.GetValue;
                _set = pi.SetValue;
                PropertyType = pi.PropertyType;
            }
            else if (memberInfo is FieldInfo fi)
            {
                _get = fi.GetValue;
                _set = fi.SetValue;
                PropertyType = fi.FieldType;
            }
            else
                throw new InvalidCastException();
        }

        public object GetValue(object obj) => _get(obj);

        public void SetValue(object obj, object value) => _set(obj, value);

        public static bool TryCreate(MemberInfo memberInfo, out EnifPropertyInfo propertyInfo)
        {
            var name = memberInfo.Name;
            var isNullable = true;
            propertyInfo = default;

            var pi = memberInfo as PropertyInfo;
            var fi = memberInfo as FieldInfo;

            // ignore property without get and set
            if (pi != null && (pi.GetMethod == null || pi.SetMethod == null || pi.GetMethod.IsStatic))
                return false;

            // ignore member with non serialized attribute
            if (memberInfo.GetCustomAttribute<NonSerializedAttribute>() != null)
                return false;

            var elementAttribute = memberInfo.GetCustomAttribute<EnifElementAttribute>();
            if (elementAttribute != null)
            {
                if (elementAttribute.ElementName != null)
                    name = elementAttribute.ElementName;

                isNullable = elementAttribute.IsNullable;
            }

            if ((pi != null && (!pi.GetMethod.IsPublic || pi.SetMethod == null || pi.GetMethod.IsStatic)) ||
                (fi != null && (!fi.IsPublic || fi.IsNotSerialized || fi.IsLiteral || fi.IsStatic || fi.IsInitOnly)))
                return false;

            propertyInfo = new(memberInfo, name, isNullable);
            return true;
        }
    }
}
