using System;
using System.Collections.Generic;
using System.Reflection;

namespace Hiku.Core
{
    public interface IDataProviderObject
    {
        Providers GetProviders();
    }

    public class Providers
    {
        List<Provider> providers;

        public IEnumerable<Provider> All => providers;

        private Providers() {}

        public Providers(params Provider[] providers)
        {
            this.providers = new List<Provider>(providers);
        }

        public static Providers Build(object obj)
        {
            var providers = new List<Provider>();

            foreach (var field in obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance))
            {
                if (ProviderBaseType.IsAssignableFrom(field.FieldType))
                {
                    var value = field.GetValue(obj) as Provider;
                    if (value == null)
                    {
                        UnityEngine.Debug.LogError($"Provider {obj.GetType().GetFriendlyName()} field uninitialized: {field.Name}");
                        //field.SetValue(obj, value = CreateInstance(field.FieldType));
                    }
                    else
                        providers.Add(value);
                }
            }

            return new Providers() { providers = providers };
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