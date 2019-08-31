using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Hiku.Core
{
    public static class DataAccessorHelper
    {
        public static Delegate CreateGetter(MethodInfo methodInfo)
        {
            return methodInfo.CreateDelegate(Expression.GetDelegateType(methodInfo.DeclaringType, methodInfo.ReturnType));
        }
    }

    /// <summary>
    /// Wraps a chain of getter methods that can be called by a certain type of object.
    /// </summary>
    public class GetterChainCall
    {
        Delegate[] accessors;

        private GetterChainCall() {}

        public static GetterChainCall Construct(Type type, string[] getters)
        {
            var accessors = new Delegate[getters.Length];
            for (int i = 0; i < getters.Length; i++)
            {
                var method = type.GetMethod(getters[i]);
                accessors[i] = DataAccessorHelper.CreateGetter(method);
                type = GetMethodReturnType(method);
            }
            return new GetterChainCall { accessors = accessors };
        }

        /// <summary>
        /// Returns an action that invokes the getter chain and passes the result 
        /// to the given delegate.
        /// </summary>
        public Action<T> Wrap<T>(Delegate d, ReceiverBuilder receivers)
        {
            IGetterMethod getter = new DelegateGetter(d);
            for (int i = accessors.Length - 1; i >= 0; i--)
            {
                //UnityEngine.Debug.Log("PATH " + path[i] + ", " + accessors[i].Method.ReturnType.GetFriendlyName());
                if (typeof(Provider).IsAssignableFrom(accessors[i].Method.ReturnType))
                    getter = new ProviderGetter(accessors[i], getter, receivers);
                else
                    getter = new ReflectionGetterMethod(accessors[i], getter);
            }
            return getter.GetDelegate<T>();
        }

        /// <summary>
        /// Returns method's return type or in case the return type is a type of Provider, 
        /// then returns the provided type.
        /// </summary>
        public static Type GetMethodReturnType(MethodInfo method)
        {
            var type = method.ReturnType;
            var providedType = Providers.GetProvidedType(type);
            return providedType ?? type;
        }
    }

    public interface IGetterMethod
    {
        Delegate Delegate { get; }
        Action<T> GetDelegate<T>();
    }

    public class DelegateGetter : IGetterMethod
    {
        Delegate d;

        public DelegateGetter(Delegate d) => this.d = d;

        public Delegate Delegate => d;

        public Action<T> GetDelegate<T>() => (Action<T>) d;
    }

    /// <summary>
    /// Normal getter methods result in dynamic invocations as the types are not known.
    /// </summary>
    public class ReflectionGetterMethod : IGetterMethod
    {
        Delegate getter;
        IGetterMethod next;

        public ReflectionGetterMethod(Delegate getter, IGetterMethod next)
        {
            this.next = next;
            this.getter = getter;
        }

        private void Apply<T>(T target)
        {
            next.Delegate.DynamicInvoke(target == null ? null : getter.DynamicInvoke(target));
        }

        public Delegate Delegate => (Action<object>) Apply;

        public Action<T> GetDelegate<T>() => Apply;
    }

    /// <summary>
    /// Getters that are types of Provider are handled differently.
    /// If all getters in a chain are types of Provider, there is no 
    /// need for dynamic invocation.
    /// </summary>
    public class ProviderGetter : IGetterMethod, ProviderListener
    {
        Delegate getter;
        IGetterMethod next;
        DataReceiver dataReceiver;
        ReceiverBuilder receivers;

        public ProviderGetter(Delegate getter, IGetterMethod next, ReceiverBuilder receivers)
        {
            this.getter = getter;
            this.next = next;
            this.receivers = receivers;
        }

        private void Apply(object target)
        {
            Provider provider = null;
            if (target != null)
                provider = getter.DynamicInvoke(target) as Provider;
            if (provider != null)
                provider.Register(this);
            else
                dataReceiver?.Dispose();
        }

        public Delegate Delegate => (Action<object>) Apply;

        public Action<T> GetDelegate<T>()
        {
            var method = (Func<T, Provider>) getter;
            return target => method.Invoke(target)?.Register(this);
        }

        void ProviderListener.RegisterWith<T>(Provider<T> provider)
        {
            var delegateReceiver = dataReceiver as DelegateReceiver<T>;
            if (delegateReceiver == null)
            {
                delegateReceiver = new DelegateReceiver<T>(next.GetDelegate<T>());
                receivers.Receive(delegateReceiver);
            }
            delegateReceiver.Register(provider);
            delegateReceiver.SetRegistered(true);
            dataReceiver = delegateReceiver;
        }
    }
}