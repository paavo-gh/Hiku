using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq.Expressions;

namespace Hiku.Core
{
    /// <summary>
    /// Caches the process of creating receivers from Unity components to 
    /// avoid the need to do that separately each time a type of component 
    /// is instantiated.
    /// </summary>
    public class ReceiversCreator
    {
        List<ReceiverCreator> creators = new List<ReceiverCreator>();

        public void Add(ReceiverCreator creator) => creators.Add(creator);

        public Receivers Create(MonoBehaviour target)
        {
            var receivers = new Receivers();
            foreach (var creator in creators)
            {
                var receiver = creator.CreateReceiver(target);
                if (receiver != null)
                    receivers.Receive(receiver);
            }
            return receivers;
        }
    }

    public class ReceiverCreator
    {
        Type type;
        Type methodType;
        MethodInfo method;
        GetterChainCall getters;

        public ReceiverCreator(MethodInfo method, Type methodType, Type type, GetterChainCall getters = null)
        {
            this.type = type;
            this.methodType = methodType;
            this.method = method;
            this.getters = getters;
        }

        public DataReceiver CreateReceiver(MonoBehaviour target)
        {
            var provider = ReceiverComponentBuilder.FindProvider(target, type);
            if (provider != null)
                return DelegateReceiver.Create(provider, this, target);
            return null;
        }

        public Action<T> CreateDelegate<T>(object target)
        {
            var d = method.CreateDelegate(Expression.GetActionType(methodType), target);

            if (getters != null)
                return getters.Wrap<T>(d);

            try
            {
                return (Action<T>) d;
            }
            catch (Exception e)
            {
                var obj = target as Component;
                Debug.LogError($"Receiver creation failed for {obj.name} ({obj.GetType().Name}:{method.Name}) when casting from {d.GetType().GetFriendlyName()} to {typeof(Action<T>).GetFriendlyName()}: {e.Message} {e.StackTrace}", obj);
                return null;
            }
        }
    }

    public class DelegateReceiver : DataReceiver
    {
        private Delegate listener;
        private Provider provider;

        private DelegateReceiver() {}

        public static DelegateReceiver Create(Provider provider, ReceiverCreator listenerProvider, object target)
        {
            var listener = provider.AddListener(new DelegateProviderImpl
            {
                ListenerProvider = listenerProvider,
                Target = target
            });
            if (listener == null)
                return null;
            return new DelegateReceiver
            {
                listener = listener,
                provider = provider
            };
        }

        public void Dispose()
        {
            provider.RemoveListener(listener);
        }

        public struct DelegateProviderImpl : DelegateProvider
        {
            public ReceiverCreator ListenerProvider;
            public object Target;

            public Action<T> GetDelegate<T>()
            {
                return ListenerProvider.CreateDelegate<T>(Target);
            }
        }
    }
}