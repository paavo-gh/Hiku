using System;
using System.Collections.Generic;
using UnityEngine;

namespace Hiku.Core
{
    public static class ReceiverComponentBuilder
    {
        public static Provider FindProvider(MonoBehaviour owner, Type type)
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
                        foreach (var provider in obj.GetProviders().All)
                        {
                            if (type.IsAssignableFrom(provider.Type))
                                return provider;
                        }
                    }
                }
                providerList.Clear();
            }
            Debug.LogError($"Unable to find {nameof(Provider)}<{type.GetFriendlyName()}> for {owner.GetType().Name} named '{owner.name}'", owner);
            return null;
        }

        /// <summary>
        /// Finds provider object without triggering any in-game logic in editor.
        /// </summary>
        public static IDataProviderObject FindProviderObject(MonoBehaviour owner, Type type)
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
                        foreach (var provider in Providers.Build(obj).All)
                        {
                            if (type.IsAssignableFrom(provider.Type))
                                return obj;
                        }
                    }
                }
                providerList.Clear();
            }
            return null;
        }
    }
}