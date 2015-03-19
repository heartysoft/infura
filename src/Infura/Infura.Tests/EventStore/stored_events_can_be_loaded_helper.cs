using System.Linq;
using Infura.Tests.EventStore;
using Machine.Specifications;

namespace Infura.Tests.EventStore
{
    public class store_events_can_be_loaded_helper
    {
        readonly object[] storedEvents;

        public store_events_can_be_loaded_helper(
            Infura.EventSourcing.EventStore eventStore)
        {
            var eventsToStore = new object[]
            {
                new SomeEvent(24, "John"),
                new SomeOtherEvent(24, "Stuart")
            };

            eventStore.StoreEvents(24, eventsToStore, 0);

            eventsToStore = new object[]
            {
                new SomeEvent(27, "Harry"),
                new SomeOtherEvent(27, "William")
            };

            eventStore.StoreEvents(27, eventsToStore, 0);

            eventsToStore = new object[]
            {
                new SomeOtherEvent(24, "Mary")
            };

            eventStore.StoreEvents(24, eventsToStore, 2);
            storedEvents = eventStore.LoadEvents(24).Result.ToArray();
        }

        public void execute()
        {
            var first = (SomeEvent)storedEvents[0];
            first.Id.ShouldEqual(24);
            first.Name.ShouldEqual("John");

            var second = (SomeOtherEvent)storedEvents[1];
            second.Id.ShouldEqual(24);
            second.NewName.ShouldEqual("Stuart");

            var third = (SomeOtherEvent)storedEvents[2];
            third.Id.ShouldEqual(24);
            third.NewName.ShouldEqual("Mary");   
        }
    }
}