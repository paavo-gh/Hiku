using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Hiku.Core
{
    public class ProvidersCreator
    {
        FieldInfo[] providerFields;

        static readonly Dictionary<Type, ProvidersCreator> creatorCache = new Dictionary<Type, ProvidersCreator>();

        public static ProvidersCreator GetCreator(Type type)
        {
            if (creatorCache.TryGetValue(type, out var creator))
                return creator;

            var providerCreator = new ProvidersCreator();

            providerCreator.providerFields = type
                .GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance)
                .Where(field => ProviderBaseType.IsAssignableFrom(field.FieldType))
                .ToArray();

            creatorCache[type] = providerCreator;

            return providerCreator;
        }

        public List<Provider> Create(object obj)
        {
            var providers = new List<Provider>();
            foreach (var field in providerFields)
            {
                var value = field.GetValue(obj) as Provider;
                if (value == null)
                    UnityEngine.Debug.LogError($"Provider {obj.GetType().GetFriendlyName()} field uninitialized: {field.Name}");
                else
                    providers.Add(value);
            }
            return providers;
        }

        /// <summary>
        /// Finds add provided types by the given provider type. Used by editor utilities.
        /// </summary>
        public static IEnumerable<Type> GetProviderTypes(Type providerType)
        {
            var types = new List<Type>();

            foreach (var field in providerType.GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                var type = GetProvidedType(field.FieldType);
                if (type != null)
                    types.Add(type);
            }

            return types;
        }

        static readonly Type ProviderBaseType = typeof(Provider);
        static readonly Type ProviderBaseGenericType = typeof(Provider<>);

        /// <summary>
        /// Returns the provided type of the given provider:
        ///   GetProviderType(typeof(Provider<T>)) == typeof(T)
        ///   GetProviderType(typeof(DataField<T>)) == typeof(T)
        /// </summary>
        public static Type GetProvidedType(Type type)
        {
            if (ProviderBaseType.IsAssignableFrom(type))
            {
                do
                {
                    if (type.IsGenericType && type.GetGenericTypeDefinition() == ProviderBaseGenericType)
                        return type.GetGenericArguments()[0];
                    type = type.BaseType;
                }
                while (type != null);
            }
            return null;
        }
    }
}