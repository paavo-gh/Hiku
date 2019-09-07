using System.Collections.Generic;

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
            => new Providers { providers = ProvidersCreator.GetCreator(obj.GetType()).Create(obj) };
    }
}