using System;
using Infura.Tests.EventStore;
using Infura.EventSourcing;
using Machine.Specifications;

namespace Infura.Tests.EventStore
{
    public class events_since_a_timestamp_can_be_loaded_helper
    {
        readonly StoredEvent[] allStoredEvents;
        readonly StoredEvent[] eventsAfterFirstCommit;
        readonly StoredEvent[] eventsAfterSecondCommit;
        readonly StoredEvent[] eventsAfterThirdCommit;
        readonly StoredEvent[] eventsFromMomentOfSecondCommit;

        public events_since_a_timestamp_can_be_loaded_helper(
           EventSourcing.EventStore eventStore, Clock clock)
        {
            clock.SetTime(new DateTime(2010, 2, 2, 10, 0, 0));

            var eventsToStore = new object[]
            {
                new SomeEvent(24, "John"),
                new SomeOtherEvent(24, "Stuart")
            };

            eventStore.StoreEvents(24, eventsToStore, 0);

            clock.SetTime(new DateTime(2010, 2, 2, 10, 0, 5));

            eventsToStore = new object[]
            {
                new SomeEvent(27, "Harry"),
                new SomeOtherEvent(27, "William")
            };

            eventStore.StoreEvents(27, eventsToStore, 0);

            clock.SetTime(new DateTime(2010, 2, 2, 10, 0, 10));

            eventsToStore = new object[]
            {
                new SomeOtherEvent(24, "Mary")
            };

            eventStore.StoreEvents(24, eventsToStore, 2);

            allStoredEvents = eventStore.GetEventsSince(DateTime.MinValue);
            eventsAfterFirstCommit = 
                eventStore.GetEventsSince(new DateTime(2010, 2, 2, 10, 0, 1));
            eventsAfterSecondCommit = 
                eventStore.GetEventsSince(new DateTime(2010, 2, 2, 10, 0, 6));
            eventsAfterThirdCommit = 
                eventStore.GetEventsSince(new DateTime(2010, 2, 2, 10, 0, 11));
            eventsFromMomentOfSecondCommit = 
                eventStore.GetEventsSince(new DateTime(2010, 2, 2, 10, 0, 5));
        }

        public void execute()
        {
            allStoredEvents.Length.ShouldEqual(5);
            eventsAfterFirstCommit.Length.ShouldEqual(3);
            eventsAfterSecondCommit.Length.ShouldEqual(1);
            eventsAfterThirdCommit.Length.ShouldEqual(0);
            eventsFromMomentOfSecondCommit.Length.ShouldEqual(3);
        }
    }

    public class Clock
    {
        DateTime _currentTime;

        public DateTime GetTime()
        {
            return _currentTime;
        }

        public void SetTime(DateTime timeToReturn)
        {
            _currentTime = timeToReturn;
        }
    }
}