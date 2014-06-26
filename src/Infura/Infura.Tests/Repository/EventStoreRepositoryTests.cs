using System;
using System.Collections.Generic;
using Infura.EventSourcing;
using Infura.Tests.Repository;
using Machine.Specifications;

namespace Infura.Tests.Repository
{
    public class EventStoreRepositorySubjectAttribute : SubjectAttribute
    {
        public EventStoreRepositorySubjectAttribute() : base("EventStoreRepository")
        {
        }
    }

    public class when_loading_aggregates
    {
        static aggregate_loading_helper helper;

        Establish context = () =>
        {
            var eventStore = new InMemoryTestEventStore();
            var repository = new EventStoreRepository(eventStore);
               
            helper = new aggregate_loading_helper(repository);
        };

        It should_return_saved_aggregate = () =>
            helper.VerifyFindingOfSavedAggregate();

        It should_not_find_unsaved_aggreagate = () =>
            helper.VerifyNotFindingUnsavedAggreagte();
    }

    public class InMemoryTestEventStore : Infura.EventSourcing.EventStore
    {
        readonly Dictionary<object, List<object>> store = new Dictionary<object, List<object>>();

        public void StoreEvents(object id, IEnumerable<object> events, long expectedInitialVersion)
        {
            if (store.ContainsKey(id))
                store[id].AddRange(events);
            else
                store[id] = new List<object>(events);
        }

        public IEnumerable<object> LoadEvents(object id, long version = 0)
        {
            if (store.ContainsKey(id))
                return store[id];

            return new object[0];
        }

        public StoredEvent[] GetEventsSince(DateTime timestamp, int maxCount = 1000)
        {
            throw new NotImplementedException();
        }

        public void AdjustDispatcher(Action<StoredEvent> dispatcher)
        {
            throw new NotImplementedException();
        }
    }
}