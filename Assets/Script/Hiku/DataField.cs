using System;
using Hiku.Core;

namespace Hiku
{
    /// <summary>
    /// Stores the data and only notifies the listeners when the data changes.
    /// </summary>
    public class DataField<T> : Provider<T>
    {
        T value;
        bool initialized;

        public DataField() {}

        public DataField(T initialValue) => Set(initialValue);

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
            bool changed = !initialized || (!val?.Equals(value) ?? true);
            initialized = true;
            value = val;
            if (changed)
                listeners.Invoke(val);
        }

        public T Get() => value;
    }
}