using System;
using UnityEngine;
using Hiku.Core;

namespace Hiku
{
    public abstract class ProviderComponent : MonoBehaviour, IDataProviderObject
    {
        Providers providers;
        [SerializeField] ProviderLinker providerLinker;

        public Providers GetProviders()
        {
            if (providers == null)
            {
                providers = Providers.Build(this);
                Initialize();
            }
            return providers;
        }

        protected virtual void Initialize() {}

        protected virtual void Awake() => GetProviders(); // If we want DataFields auto-assigned
    }
}