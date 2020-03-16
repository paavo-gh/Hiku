using System;
using System.Collections.Generic;
using Hiku.Core;

namespace Hiku
{
    /// <summary>
    /// Provides data of type T to it's listeners.
    /// </summary>
    public abstract class Provider<T> : Provider
    {
        public abstract event Action<T> Listeners;

        Type Provider.Type => typeof(T);

        void Provider.Register(ProviderListener providerListener) => providerListener.RegisterWith(this);

        public abstract T Get();
    }

    /// <summary>
    /// Stores the data and only notifies the listeners when the data changes.
    /// </summary>
    public class DataField<T> : Provider<T>
    {
        T value;
        bool initialized;
        Func<T, T, bool> comparer = EqualityComparer<T>.Default.Equals;

        public DataField() {}

        /// <summary>
        /// DataField will only notify the listeners if equalityComparer(oldValue, newValue) = false.
        /// </summary>
        public DataField(Func<T, T, bool> equalityComparer)
        {
            this.comparer = equalityComparer;
        }

        public DataField(T initialValue, Func<T, T, bool> comparer = null)
        {
            if (comparer != null)
                this.comparer = comparer;
            Set(initialValue);
        }

        // Events do not work with different parameter types
        private Listeners<T> listeners = new Listeners<T>();
        public override event Action<T> Listeners
        {
            add
            {
                listeners.Add(value);
                if (initialized)
                    value(Get());
            }
            remove => listeners.Remove(value);
        }

        public void Set(T val)
        {
            bool changed = !initialized || !comparer(value, val);
            initialized = true;
            value = val;
            if (changed)
                listeners.Invoke(val);
        }

        public override T Get() => value;
    }
}