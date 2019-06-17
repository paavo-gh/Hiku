using System;
using System.Collections.Generic;

namespace Hiku.Core
{
    /// <summary>
    /// Ensures the listeners will be invoked in order even when invoked recursively.
    /// </summary>
    public class Listeners<T>
    {
        private List<Action<T>> listeners = new List<Action<T>>();
        private Action continueWith;

        public void Add(Action<T> listener) => listeners.Add(listener);

        public void Remove(Action<T> listener)
        {
            for (int i = 0; i < listeners.Count; i++)
                if (listeners[i] == listener)
                    listeners[i] = null;
        }

        public void Invoke(T arg)
        {
            // Can only happen when Invoke is called recursively
            if (continueWith != null)
            {
                var action = continueWith;
                continueWith = () =>
                {
                    action();
                    Invoke(arg);
                };
                return;
            }

            try
            {
                continueWith = Nothing;
                
                int count = listeners.Count;
                for (int i = 0; i < count; i++)
                {
                    if (listeners[i] != null)
                        listeners[i].Invoke(arg);
                    else
                    {
                        listeners.RemoveAt(i--);
                        count--;
                    }
                }
            }
            finally
            {
                var invoke = continueWith;
                continueWith = null;
                invoke();
            }
        }

        private void Nothing() {}
    }
}