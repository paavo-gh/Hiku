
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Hiku.Core;

namespace Hiku
{
    /// <summary>
    /// Base class for components that can be both a provider and receiver.
    /// </summary>
    public abstract class ProviderReceiverComponent : ProviderComponent, IDataProviderObject
    {
        [SerializeField] ReceiverLinker linker;
        Receivers receivers;

        protected override void Awake()
        {
            base.Awake();
            receivers = linker.Build(this);
        }

        protected virtual void OnDestroy() => receivers?.Dispose();
    }
}