using System;
using System.Collections.Generic;
using System.Linq;

namespace Hiku.Core
{
    public static class TypeTranslator
    {
        private static Dictionary<Type, string> translations = new Dictionary<System.Type, string>
        {
            {typeof(int), "int"},
            {typeof(uint), "uint"},
            {typeof(long), "long"},
            {typeof(ulong), "ulong"},
            {typeof(short), "short"},
            {typeof(ushort), "ushort"},
            {typeof(byte), "byte"},
            {typeof(sbyte), "sbyte"},
            {typeof(bool), "bool"},
            {typeof(float), "float"},
            {typeof(double), "double"},
            {typeof(decimal), "decimal"},
            {typeof(char), "char"},
            {typeof(string), "string"},
            {typeof(object), "object"},
            {typeof(void), "void"}
        };

        public static string GetFriendlyName(this Type type, int maxDepth = 3)
        {
            if (translations.ContainsKey(type))
                return translations[type];
            else if (type.IsArray)
                return GetFriendlyName(type.GetElementType()) + "[]";
            else if(type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
                return type.GetGenericArguments()[0].GetFriendlyName() + "?";
            else if (type.IsGenericType)
            {
                var baseName = type.Name.Split('`')[0];
                if (maxDepth > 0)
                    return baseName + "<" + string.Join(", ", type.GetGenericArguments().Select(x => GetFriendlyName(x, maxDepth - 1))) + ">";
                return baseName + "<>";
            }
            else
                return type.Name;
        }
    }
}