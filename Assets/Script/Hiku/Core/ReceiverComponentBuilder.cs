using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hiku.Core
{
    public static class ReceiverComponentBuilder
    {
        public static Provider FindProvider(MonoBehaviour owner, Type type)
        {
            var result = findProvider(owner, type);
            if (result.HasValue)
                return result.Value.Item2;
            Debug.LogError($"Unable to find {nameof(Provider)}<{type.GetFriendlyName()}> for {owner.GetType().Name} named '{owner.name}'", owner);
            return null;
        }

        public static IDataProviderObject FindProviderObject(MonoBehaviour owner, Type type)
        {
            var result = findProvider(owner, type);
            if (result.HasValue)
                return result.Value.Item1;
            return null;
        }

        private static (IDataProviderObject, Provider)? findProvider(MonoBehaviour owner, Type type)
        {
            var providerList = new List<IDataProviderObject>();
            for (var t = owner.transform; t != null; t = t.parent)
            {
                t.GetComponents<IDataProviderObject>(providerList);
                for (int i = 0; i < providerList.Count; i++)
                {
                    if (object.ReferenceEquals(providerList[i], owner))
                        continue;
                    
                    foreach (var provider in providerList[i].GetProviders().All)
                    {
                        if (type.IsAssignableFrom(provider.Type))
                            return (providerList[i], provider);
                    }
                }
                providerList.Clear();
            }
            return null;
        }
    }
}