using System;

namespace Hiku.Core
{
    /// <summary>
    /// Provides certain type of data to its listeners.
    /// Needed by reflection when the type is not known.
    /// </summary>
    public interface Provider
    {
        Type Type { get; }

        /// <summary>
        /// This way the delegate can be created or casted to the correct 
        /// type to avoid unnecessary casting when notifying the listener.
        /// </summary>
        void Register(ProviderListener providerListener);
    }

    public interface ProviderListener
    {
        void RegisterWith<T>(Provider<T> provider);
    }
}