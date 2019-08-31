using System;

namespace Hiku.Examples
{
    // ProviderComponents provide certain data types to their child objects
    public class PlayerProvider : ProviderComponent, IPlayerService, ITimeComponent
    {
        // Values of these DataFields are provided to all child objects expecting such a type
        DataField<Player> player = new DataField<Player>();
        DataField<IPlayerService> playerService = new DataField<IPlayerService>();
        DataField<ITimeComponent> timeComponent = new DataField<ITimeComponent>();
        DataField<DateTime> timerTest = new DataField<DateTime>();

        public DateTime CurrentTime => DateTime.UtcNow;

        // Initialize is called before any data is provided by the component
        protected override void Create()
        {
            player.Set(new Player { Id = "234237" });
            playerService.Set(this);
            timeComponent.Set(this);
            timerTest.Set(CurrentTime.AddHours(1));
        }

        public void ChangeName(string name)
        {
            player.Get().Name.Set(name);
        }
    }

    // Receivable-tag tells the editor scripts that any properties and getter methods 
    // of the tagged class should be selectable from the editor popup menu.
    [Receivable]
    public class Player
    {
        public string Id { get; set; }

        // DataFields work fine outside of components.
        // All listening components be notified when the value changes.
        public DataField<string> Name { get; set; }

        public Player()
        {
            Name = new DataField<string>();
        }
    }

    public interface IPlayerService
    {
        void ChangeName(string name);
    }

    public interface ITimeComponent
    {
        DateTime CurrentTime { get; }
    }
}