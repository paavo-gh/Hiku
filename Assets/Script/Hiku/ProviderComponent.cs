using UnityEngine;
using Hiku.Core;

namespace Hiku
{
    public abstract class ProviderComponent : MonoBehaviour, IDataProviderObject
    {
        Providers providers;
        [SerializeField] ProviderLinker providerLinker;
        bool initializing;

        public Providers GetProviders()
        {
            if (providers == null)
            {
                initializing = true;
                providers = Providers.Build(this);
                Init();
                initializing = false;
            }
            if (initializing)
                Debug.LogError("Potential inheritance loop"); // TODO These shouldn't happen  anymore
            return providers;
        }

        protected virtual void Init() => Create();

        protected virtual void OnEnable()
        {
            GetProviders();
            Enable();
        }

        protected virtual void OnDisable() => Disable();

        /// <summary>
        /// Triggers after the component has been created and all the providers and 
        /// receivers have been initialized.
        /// </summary>
        protected virtual void Create() {}

        /// <summary>
        /// Triggers after the component has been enabled and all the providers and 
        /// receivers have been initialized.
        /// </summary>
        protected virtual void Enable() {}

        /// <summary>
        /// Triggers when the component is disabled.
        /// </summary>
        protected virtual void Disable() {}
    }
}