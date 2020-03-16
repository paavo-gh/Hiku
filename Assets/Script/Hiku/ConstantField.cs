using System;
using Hiku.Core;

namespace Hiku
{
    /// <summary>
    /// Stores a constant value. Doesn't keep track of the listeners.
    /// </summary>
    public class ConstantField<T> : Provider<T>
    {
        readonly T value;

        public ConstantField(T initialValue) => value = initialValue;

        public override event Action<T> Listeners
        {
            add => value(Get());
            remove {}
        }

        public override T Get() => value;
    }
}