using Infura.Tests.EventStore;
using Machine.Specifications;

namespace Infura.Tests.EventStore
{
    public class events_are_dispatched_as_they_are_added_helper
    {
        int dispatchedMessageCount;

        public events_are_dispatched_as_they_are_added_helper(
            Infura.EventSourcing.EventStore eventStore)
        {
            eventStore.AdjustDispatcher(x => dispatchedMessageCount++);

            var eventsToStore = new object[]
            {
                new SomeEvent(24, "John"),
                new SomeOtherEvent(24, "Stuart")
            };

            eventStore.StoreEvents(24, eventsToStore, 0);
        }

        public void execute()
        {
            dispatchedMessageCount.ShouldEqual(2);
        }
    }
}