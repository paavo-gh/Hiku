using System;
using UnityEngine;
using Hiku.Core;

namespace Hiku
{
    public class ReceiverComponent : MonoBehaviour
    {
        public ReceiverLinker linker;
        Receivers receivers;

        protected virtual void Awake() => receivers = linker.Build(this);

        protected virtual void OnDestroy() => receivers?.Dispose();
    }
}