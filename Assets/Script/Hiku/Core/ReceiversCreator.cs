using UnityEngine;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq.Expressions;

namespace Hiku.Core
{
    /// <summary>
    /// Caches the process of creating receivers from Unity components.
    /// </summary>
    public class ReceiversCreator
    {
        List<ReceiverCreator> creators = new List<ReceiverCreator>();

        public void Add(ReceiverCreator creator) => creators.Add(creator);

        public Receivers Create(MonoBehaviour target)
        {
            var receivers = new Receivers();
            foreach (var creator in creators)
                creator.CreateReceiver(target, receivers);
            return receivers;
        }
    }

    public class ReceiverCreator
    {
        Type type;
        Type methodType;
        MethodInfo method;
        GetterChainCall getters;
        public readonly Receive Attribute;

        public ReceiverCreator(ReceiverMethod receiverMethod, Type type = null, GetterChainCall getters = null)
        {
            this.type = type ?? receiverMethod.Type;
            this.methodType = receiverMethod.Type;
            this.method = receiverMethod.Method;
            this.getters = getters;
            this.Attribute = receiverMethod.Attribute;
        }

        public void CreateReceiver(MonoBehaviour target, ReceiverBuilder receivers)
        {
            var provider = ReceiverComponentBuilder.FindProvider(target, type);
            if (provider != null)
                DelegateReceiverCreator.Create(provider, this, target, receivers);
        }

        public Action<T> CreateDelegate<T>(object target, ReceiverBuilder receivers)
        {
            var d = method.CreateDelegate(Expression.GetActionType(methodType), target);

            if (getters != null)
                return getters.Wrap<T>(d, receivers);

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

    public class DelegateReceiverCreator : ProviderListener
    {
        ReceiverCreator receiverCreator;
        object target;
        ReceiverBuilder receivers;

        private DelegateReceiverCreator(ReceiverCreator receiverCreator, object Target, ReceiverBuilder receivers)
        {
            this.receiverCreator = receiverCreator;
            this.target = Target;
            this.receivers = receivers;
        }

        void ProviderListener.RegisterWith<T>(Provider<T> provider)
        {
            var action = receiverCreator.CreateDelegate<T>(target, receivers);
            if (action != null)
            {
                var delegateReceiver = new DelegateReceiver<T>(action);
                receivers.Receive(delegateReceiver);
                delegateReceiver.Register(provider);
            }
        }

        public static void Create(Provider provider, ReceiverCreator receiverCreator, object target, ReceiverBuilder receivers)
        {
            var delegateProvider = new DelegateReceiverCreator(receiverCreator, target, receivers);
            provider.Register(delegateProvider);
        }
    }

    public class DelegateReceiver<T> : DataReceiver
    {
        T value;
        Action<T> delegateMethod;
        bool registered;
        bool pendingValue;

        Provider<T> provider;

        public DelegateReceiver(Action<T> delegateMethod)
        {
            this.delegateMethod = delegateMethod;
        }

        public void Register(Provider<T> provider)
        {
            if (this.provider != null)
                this.provider.Listeners -= ValueChanged;

            this.provider = provider;
            provider.Listeners += ValueChanged;
        }

        void ValueChanged(T val)
        {
            if (registered)
                delegateMethod(val);
            else
            {
                pendingValue = true;
                value = val;
            }
        }

        public void SetRegistered(bool registered)
        {
            this.registered = registered;
            if (registered && pendingValue)
            {
                pendingValue = false;
                delegateMethod(value);
            }
        }

        void DataReceiver.Dispose() => provider.Listeners -= ValueChanged;
    }
}