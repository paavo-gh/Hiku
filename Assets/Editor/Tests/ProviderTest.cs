using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace Hiku.Examples.Order
{
    public class ProviderTest : ProviderComponent
    {
        DataField<string> stringProvider;
        DataField<ProviderTestObject> testObjectProvider;

        protected override void Create()
        {
            Update();
            // Called once when the component has been created.
            // Guarantees that all available data has been received.
        }

        void Update()
        {
            if (Random.value < 0.2f || testObjectProvider.Get() == null)
                testObjectProvider.Set(new ProviderTestObject());
            testObjectProvider.Get().RandomizeDataFields();
            stringProvider.Set(Random.value.ToString());
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

    [Receivable]
    public class ProviderTestObject
    {
        public DataField<string> Name { get; private set; }
        public DataField<int> Age { get; private set; }
        public DataField<PlaceTestObject> Place { get; private set; }
        public PlaceTestObject PersistentPlace { get; private set; }
        
        public ProviderTestObject()
        {
            Name = new DataField<string>();
            Age = new DataField<int>();
            Place = new DataField<PlaceTestObject>();
            PersistentPlace = new PlaceTestObject();
        }

        public void RandomizeDataFields()
        {
            Name.Set(Random.value.ToString());
            Age.Set(Random.Range(int.MinValue, int.MaxValue));
            if (Random.value < 0.3f)
                Place.Set(new PlaceTestObject());
            Place.Get().RandomizeDataFields();
            PersistentPlace.RandomizeDataFields();
        }
    }

    [Receivable]
    public class PlaceTestObject
    {
        public DataField<string> Name { get; private set; }
        public string PersistentName { get; private set; }
        public DataField<int> Number { get; private set; }
        
        public PlaceTestObject()
        {
            Name = new DataField<string>();
            Number = new DataField<int>();
            PersistentName = Random.value.ToString();
        }

        public void RandomizeDataFields()
        {
            Name.Set(Random.value.ToString());
            Number.Set(Random.Range(int.MinValue, int.MaxValue));
        }
    }
}