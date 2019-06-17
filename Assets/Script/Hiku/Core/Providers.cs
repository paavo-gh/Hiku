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
                if (typeof(Provider).IsAssignableFrom(field.FieldType))
                {
                    var value = field.GetValue(obj) as Provider;
                    if (value == null)
                        field.SetValue(obj, value = CreateInstance(field.FieldType));
                    providers.Add(value);
                }
            }

            return new Providers() { providers = providers };
        }

        public static Provider CreateInstance(Type providerType)
        {
            try
            {
                return (Provider) Activator.CreateInstance(providerType);
            }
            catch (MissingMethodException)
            {
                throw new MissingMethodException(providerType.GetFriendlyName() + " should have a zero parameter constructor");
            }
        }
    }
}