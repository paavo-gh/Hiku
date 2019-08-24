
using UnityEngine;
using Hiku.Core;

namespace Hiku
{
    /// <summary>
    /// Base class for components that can be both a provider and receiver.
    /// </summary>
    public abstract class ProviderReceiverComponent : ProviderComponent, IDataProviderObject
    {
        [SerializeField] ReceiverLinker linker = null;
        Receivers receivers;

        protected override sealed void Init()
        {
            receivers = linker.Build(this);
            receivers.SetRegistered(true);
            base.Init();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            receivers.SetRegistered(true);
        }

        protected override void OnDisable()
        {
            receivers.SetRegistered(false);
            base.OnDisable();
        }

        protected virtual void OnDestroy() => receivers?.Dispose();
    }
}