using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace Hiku.Examples.Order
{
    public class OrderTest : ReceiverComponent
    {
        [Receive] void SetStringValue(string val)
        {
            Assert.True(enabled);
            if (Random.value < 0.2f)
            {
                enabled = false;
                Activate();
            }
        }

        [Receive] void SetIntValue(int val) => SetStringValue("");

        async void Activate()
        {
            await Task.Delay(100);
            enabled = true;
        }

        protected override void Create()
        {
            // Called once when the component has been created.
            // Guarantees that all available data has been received.
        }

        protected override void Enable()
        {
            // Executes after Create and every time the component is enabled, like Unity's OnEnable.
        }

        protected override void Disable()
        {
            // Component will not receive any data updates while it is disabled, 
            // Any data that changes while the component is disabled, 
            // will be received the moment the component is enabled again.
        }
    }
}