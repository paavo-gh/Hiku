using System;

namespace Hiku.Core
{
    /// <summary>
    /// Provides certain type of data to its listeners.
    /// </summary>
    public interface Provider
    {
        Type Type { get; }

        /// <summary>
        /// Adds a delegate, created by the given DelegateProvider, 
        /// to listen the Provider. Returns the added listener.
        /// This way the delegate can be created or casted to the correct 
        /// type to avoid unnecessary casting when notifying the listener.
        /// </summary>
        Delegate AddListener(DelegateProvider d);

        void RemoveListener(Delegate d);
    }

    /// <summary>
    /// Simplified interface to add listeners where the type is known.
    /// </summary>
    public interface Provider<out T> : Provider
    {
        event Action<T> Listeners;
    }

    public interface DelegateProvider
    {
        /// <summary>
        /// Returns the delegate casted to the given type.
        /// </summary>
        Action<T> GetDelegate<T>();
    }
}