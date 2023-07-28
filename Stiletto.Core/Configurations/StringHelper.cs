using System;
using System.ComponentModel;

namespace Stiletto.Configurations
{
    public static class StringHelper
    {
        public static object Convert(string stringValue, Type type)
        {
            if (type == typeof(string))
            {
                return stringValue;
            }

            var nullableType = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
            if (nullableType)
            {
                if (string.IsNullOrEmpty(stringValue))
                {
                    return GetDefault(type);
                }
                type = new NullableConverter(type).UnderlyingType;
            }

            Type[] argTypes = { typeof(string), type.MakeByRefType() };
            var tryParseMethodInfo = type.GetMethod("TryParse", argTypes);
            if (tryParseMethodInfo == null)
            {
                return GetDefault(type);
            }

            object[] args = { stringValue, GetDefault(type) };
            tryParseMethodInfo.Invoke(null, args);

            return args[1];
        }

        private static object GetDefault(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }
    }
}
