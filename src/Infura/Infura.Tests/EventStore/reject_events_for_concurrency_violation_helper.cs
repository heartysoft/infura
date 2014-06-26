using System;
using System.Linq;
using Infura.Tests.EventStore;
using Machine.Specifications;

namespace Infura.Tests.EventStore
{
    public class reject_events_for_concurrency_violation_helper
    {
        readonly Exception exception;
        readonly Exception exception2;
        readonly object[] storedEvents;

        public reject_events_for_concurrency_violation_helper(
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
                new SomeEvent(24, "Harry"),
                new SomeOtherEvent(24, "William")
            };

            try
            {
                eventStore.StoreEvents(24, eventsToStore, 0);
            }
            catch (Exception e)
            {
                exception = e;
            }

            eventStore.StoreEvents(24, eventsToStore, 2);

            try
            {
                eventStore.StoreEvents(24, eventsToStore, 2);
            }
            catch (Exception e)
            {
                exception2 = e;
            }

            storedEvents = eventStore.LoadEvents(24).ToArray();
        }

        public void execute()
        {
            exception.ShouldBeOfExactType<ConcurrencyException>();
            exception2.ShouldBeOfExactType<ConcurrencyException>();

            var first = (SomeEvent)storedEvents[0];
            first.Id.ShouldEqual(24);
            first.Name.ShouldEqual("John");

            var second = (SomeOtherEvent)storedEvents[1];
            second.Id.ShouldEqual(24);
            second.NewName.ShouldEqual("Stuart");

            var third = (SomeEvent)storedEvents[2];
            third.Id.ShouldEqual(24);
            third.Name.ShouldEqual("Harry");

            var fourth = (SomeOtherEvent)storedEvents[3];
            fourth.Id.ShouldEqual(24);
            fourth.NewName.ShouldEqual("William");
        }
    } 
}