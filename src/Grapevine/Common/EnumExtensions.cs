using System;
using System.Linq;
using Grapevine.Properties;

namespace Grapevine.Common
{
    public static class EnumExtensions
    {
        public static TAttribute GetEnumAttribute<TAttribute>(this Enum enumValue) where TAttribute : Attribute
        {
            var memberInfo = enumValue.GetType().GetMember(enumValue.ToString());

            var attr = memberInfo[0]
                .GetCustomAttributes(typeof(TAttribute), false)
                .OfType<TAttribute>()
                .FirstOrDefault();

            return attr ?? (TAttribute)Activator.CreateInstance(typeof(TAttribute));
        }

        public static TAttribute[] GetEnumAttributes<TAttribute>(this Enum enumValue) where TAttribute : Attribute
        {
            var memberInfo = enumValue.GetType().GetMember(enumValue.ToString());

            return memberInfo[0]
                .GetCustomAttributes(typeof(TAttribute), false)
                .OfType<TAttribute>()
                .ToArray();
        }

        public static T FromString<T>(string value) where T : struct, IComparable, IFormattable, IConvertible
        {
            if (!typeof(T).IsEnum) throw new ArgumentException(Messages.NotEnumeratedType);

            try
            {
                return (T)Enum.Parse(typeof(T), value, true);
            }
            catch
            {
                return (T)Activator.CreateInstance(typeof(T));
            }
        }
    }
}
