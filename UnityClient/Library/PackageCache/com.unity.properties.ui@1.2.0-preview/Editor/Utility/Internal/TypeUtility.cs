using System;
using System.Linq;

namespace Unity.Properties.UI.Internal
{
    static class TypeUtility
    {
        public static string GetResolvedTypeName(Type type)
        {
            if (type == typeof(int))
                return "int";
            if (type == typeof(uint))
                return "uint";
            if (type == typeof(short))
                return "short";
            if (type == typeof(ushort))
                return "ushort";
            if (type == typeof(byte))
                return "byte";
            if (type == typeof(char))
                return "char";
            if (type == typeof(bool)) 
                return "bool";
            if (type == typeof(long))
                return "long";
            if (type == typeof(ulong))
                return "ulong";
            if (type == typeof(float))
                return "float";
            if (type == typeof(double))
                return "double";
            if (type == typeof(string))
                return "string";
            if (type.IsGenericType && type.Name.Contains('`'))
                return type.Name.Split('`')[0] + "<" + string.Join(", ", type.GetGenericArguments().Select(GetResolvedTypeName).ToArray()) + ">";

            return type.Name;
        }
    }
}