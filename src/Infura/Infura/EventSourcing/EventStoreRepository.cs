using System.Linq;

namespace Infura.EventSourcing
{
    public class EventStoreRepository : Repository
    {
        readonly EventStore _eventStore;

        public EventStoreRepository(EventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public T GetById<T>(object id) where T : Aggregate
        {
            var events = _eventStore.LoadEvents(id).ToArray();

            if (events.Length == 0)
                throw new AggregateNotFoundException();
            
            var instance = Aggregate.InitializeFromHistory<T>(id, events);
            return instance;
        }

        public void Save(Aggregate instance) 
        {
            var newEvents = instance.GetUncommittedEvents();
            var currentVersion = instance.Version;
            var initialVersion = currentVersion - newEvents.Length;

            _eventStore.StoreEvents(instance.Id, newEvents, initialVersion);
            instance.ClearUncommittedChanges();
        }
    }
}