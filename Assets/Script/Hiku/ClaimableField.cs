using System;
using Hiku.Core;

namespace Hiku
{
    [Receivable]
    public class ClaimableData<T>
    {
        public T ClaimedValue { get; private set; }
        public T Value { get; private set; }

        public ClaimableData(T value, T claimedValue)
        {
            Value = value;
            ClaimedValue = claimedValue;
        }

        public void Claim()
        {
            //if (!Value?.Equals(ClaimedValue) ?? true)
            ClaimedValue = Value;
        }
    }

    public class ClaimableField<T> : Provider<ClaimableData<T>>, Provider
    {
        readonly Func<T> Getter;
        readonly Action<T> Setter;
        ClaimableData<T> data;

        public ClaimableField()
        {
        }

        public ClaimableField(Func<T> getter, Action<T> setter)
        {
            Getter = getter;
            Setter = setter;
            data = new ClaimableData<T>(Getter(), Getter());
        }

        Type Provider.Type => typeof(ClaimableData<T>);

        // Events do not work with different parameter types
        private Listeners<ClaimableData<T>> listeners = new Listeners<ClaimableData<T>>();
        public event Action<ClaimableData<T>> Listeners
        {
            add
            {
                listeners.Add(value);
                value(data);
            }
            remove => listeners.Remove(value);
        }

        void Provider.Register(ProviderListener providerListener)
            => providerListener.RegisterWith(this);
        
        public T Value
        {
            set
            {
                Setter(value);
                data = new ClaimableData<T>(Getter(), data.ClaimedValue);
                listeners.Invoke(data);
            }
            get => Getter();
        }

        public T ClaimedValue => data.ClaimedValue;
    }
}