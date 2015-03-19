using System.Linq;
using System.Threading.Tasks;

namespace Infura.EventSourcing
{
    public class EventStoreRepository : Repository
    {
        readonly EventStore _eventStore;

        public EventStoreRepository(EventStore eventStore)
        {
            _eventStore = eventStore;
        }

        public async Task<T> GetById<T>(object id) where T : Aggregate
        {
            var events = (await _eventStore.LoadEvents(id)).ToArray();

            if (events.Length == 0)
                throw new AggregateNotFoundException();
            
            var instance = Aggregate.InitializeFromHistory<T>(id, events);
            return instance;
        }

        public async Task Save(Aggregate instance) 
        {
            var newEvents = instance.GetUncommittedEvents();
            var currentVersion = instance.Version;
            var initialVersion = currentVersion - newEvents.Length;

            await _eventStore.StoreEvents(instance.Id, newEvents, initialVersion + 1);
            instance.ClearUncommittedChanges();
        }
    }
}