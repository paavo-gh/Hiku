using UnityEngine;
using Hiku.Core;

namespace Hiku
{
    public class ReceiverComponent : MonoBehaviour
    {
        [SerializeField] ReceiverLinker linker = null;
        Receivers receivers;

        protected virtual void OnEnable()
        {
            if (receivers == null)
            {
                if (linker == null)
                    linker = new ReceiverLinker();
                receivers = linker.Build(this);
                receivers.SetRegistered(true);
                Create();
            }
            else
                receivers.SetRegistered(true);
            Enable();
        }

        protected virtual void OnDisable()
        {
            receivers.SetRegistered(false);
            Disable();
        }

        protected virtual void OnDestroy() => receivers?.Dispose();

        protected virtual void Create() {}

        protected virtual void Enable() {}

        protected virtual void Disable() {}
    }
}