using System;

namespace Ez.Enif.Serialization
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class EnifElementAttribute : EnifAttribute
    {
        public EnifElementAttribute(string customName = null, bool isNullable = false)
        {
            ElementName = customName;
            IsNullable = isNullable;
        }

        public string ElementName { get; }
        public bool IsNullable { get; }
    }
}
