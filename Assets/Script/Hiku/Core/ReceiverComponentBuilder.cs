using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Hiku.Core
{
    public static class ReceiverComponentBuilder
    {
        public static Provider TypeProvider(IDataProviderObject obj, Func<Type, bool> typeFilter)
        {
            foreach (var provider in obj.GetProviders().All)
            {
                if (typeFilter(provider.Type))
                    return provider;
            }
            return null;
        }

        public static IDataProviderObject TypeDataProviderObject(IDataProviderObject obj, Func<Type, bool> typeFilter)
            => ProvidersCreator.GetProviderTypes(obj.GetType()).Any(typeFilter) ? obj : null;

        public static T FindProvider<T>(MonoBehaviour owner, Type type, Func<IDataProviderObject, Func<Type, bool>, T> filter)
        {
            var providerList = new List<MonoBehaviour>();
            for (var t = owner.transform; t != null; t = t.parent)
            {
                t.GetComponents(providerList);
                for (int i = 0; i < providerList.Count; i++)
                {
                    // To avoid potential loops, disallow inherting from components 
                    // that are below the owner. 
                    if (object.ReferenceEquals(providerList[i], owner))
                        break;
                    
                    var obj = providerList[i] as IDataProviderObject;
                    if (obj != null)
                    {
                        var result = filter(obj, type.IsAssignableFrom);
                        if (result != null)
                            return result;
                    }
                }
                providerList.Clear();
            }
            return default(T);
        }
    }
}