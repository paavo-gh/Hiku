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
            IDelegateProvider getter = new DelegateGetter(d);
            for (int i = accessors.Length - 1; i >= 0; i--)
            {
                if (typeof(Provider).IsAssignableFrom(accessors[i].Method.ReturnType))
                    getter = new ProviderGetter(accessors[i], getter, receivers);
                else
                    getter = new ReflectionGetterMethod(accessors[i], getter);
            }
            return getter.GetTypedDelegate<T>();
        }

        /// <summary>
        /// Returns method's return type or in case the return type is a type of Provider, 
        /// then returns the provided type.
        /// </summary>
        public static Type GetMethodReturnType(MethodInfo method)
        {
            var type = method.ReturnType;
            var providedType = ProvidersCreator.GetProvidedType(type);
            return providedType ?? type;
        }
    }

    public interface IDelegateProvider
    {
        Delegate Delegate { get; }
    }

    public interface IDelegateProviderGeneric
    {
        Action<T> GetDelegate<T>();
    }

    public static class IDelegateProviderExt
    {
        public static Action<T> GetTypedDelegate<T>(this IDelegateProvider getter)
        {
            if (getter is IDelegateProviderGeneric m)
                try { return m.GetDelegate<T>(); }
#pragma warning disable 0618
                catch (ExecutionEngineException e)
#pragma warning restore 0618
                {
                    // Check AOT code generation logic
                    UnityEngine.Debug.LogError("Fallback to dynamic invocation: " + e.Message + e.StackTrace);
                }
            return (Action<T>) getter.Delegate;
        }
    }

    public class DelegateGetter : IDelegateProvider
    {
        public Delegate Delegate { get; private set; }

        public DelegateGetter(Delegate d) => this.Delegate = d;
    }

    /// <summary>
    /// Normal getter methods result in dynamic invocations as the types are not known.
    /// </summary>
    public class ReflectionGetterMethod : IDelegateProvider
    {
        Delegate getter;
        IDelegateProvider next;

        public ReflectionGetterMethod(Delegate getter, IDelegateProvider next)
        {
            this.next = next;
            this.getter = getter;
            this.Delegate = (Action<object>) Apply;
        }

        private void Apply(object target)
        {
            next.Delegate.DynamicInvoke(target == null ? null : getter.DynamicInvoke(target));
        }

        public Delegate Delegate { get; private set; }
    }

    /// <summary>
    /// Getters that are types of Provider are handled differently.
    /// If all getters in a chain are types of Provider, there is no 
    /// need for dynamic invocation.
    /// </summary>
    public class ProviderGetter : IDelegateProviderGeneric, IDelegateProvider, ProviderListener
    {
        Delegate getter;
        IDelegateProvider next;
        DataReceiver dataReceiver;
        ReceiverBuilder receivers;

        public ProviderGetter(Delegate getter, IDelegateProvider next, ReceiverBuilder receivers)
        {
            this.getter = getter;
            this.next = next;
            this.receivers = receivers;
            this.Delegate = (Action<object>) Apply;
        }

        private void Apply(object target)
        {
            dataReceiver?.Dispose();
            if (target != null)
                (getter.DynamicInvoke(target) as Provider)?.Register(this);
        }

        public Delegate Delegate { get; private set; }

        public Action<T> GetDelegate<T>()
        {
            var method = (Func<T, Provider>) getter;
            return target => {
                dataReceiver?.Dispose();
                if (target != null)
                    method.Invoke(target)?.Register(this);
            };
        }

        void ProviderListener.RegisterWith<T>(Provider<T> provider)
        {
            var delegateReceiver = dataReceiver as DelegateReceiver<T>;
            if (delegateReceiver == null)
            {
                delegateReceiver = new DelegateReceiver<T>(next.GetTypedDelegate<T>());
                receivers.Receive(delegateReceiver);
            }
            delegateReceiver.Register(provider);
            delegateReceiver.SetRegistered(true);
            dataReceiver = delegateReceiver;
        }
    }
}