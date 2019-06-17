using System;
using System.Collections.Generic;
using Hiku.Core;

namespace Hiku
{
    public class Channel<T> : Provider<T>
    {
        public Type Type => typeof(T);

        private Listeners<T> listeners = new Listeners<T>();
        public event Action<T> Listeners
        {
            add => listeners.Add(value);
            remove => listeners.Remove(value);
        }

        public Delegate AddListener(DelegateProvider dp)
        {
            var action = dp.GetDelegate<T>();
            Listeners += action;
            return action;
        }

        public void RemoveListener(Delegate d) => Listeners -= (Action<T>) d;

        public void Set(T val) => listeners.Invoke(val);
    }

    /// <summary>
    /// Stores the data and only notifies the listeners when the data changes.
    /// </summary>
    public class DataField<T> : Provider<T>
    {
        T value;
        bool initialized;

        public Type Type => typeof(T);

        // Events do not work with different parameter types
        private Listeners<T> listeners = new Listeners<T>();
        public event Action<T> Listeners
        {
            add
            {
                listeners.Add(value);
                if (initialized)
                    value(Get());
            }
            remove => listeners.Remove(value);
        }

        public Delegate AddListener(DelegateProvider dp)
        {
            var action = dp.GetDelegate<T>();
            Listeners += action;
            return action;
        }

        public void RemoveListener(Delegate d) => Listeners -= (Action<T>) d;

        public void Set(T val)
        {
            bool changed = !initialized || (!val?.Equals(value) ?? true);
            initialized = true;
            value = val;
            if (changed)
                listeners.Invoke(val);
        }

        public T Get() => value;
    }
}