using System;
using System.Collections.Generic;
using System.Linq;

namespace Infura.EventSourcing
{
    public class InMemoryEventStore : EventStore
    {
        readonly Dictionary<object, List<StoredEvent>> _store = new Dictionary<object, List<StoredEvent>>();
        private Func<DateTime> _clock = () => DateTime.UtcNow;
        private Action<StoredEvent> _dispatcher = x => { };

        public void StoreEvents(object id, IEnumerable<object> events, long expectedInitialVersion)
        {
            var storedEvents = getStoredEvents(id, events, expectedInitialVersion);

            if (_store.ContainsKey(id))
                _store[id].AddRange(storedEvents);
            else
                _store[id] = new List<StoredEvent>(storedEvents);

            foreach (var @event in storedEvents)
            {
                _dispatcher(@event);
            }
        }

        public IEnumerable<object> LoadEvents(object id, long version = 0)
        {
            if (_store.ContainsKey(id))
                return _store[id].Select(x => x.EventData).ToArray();

            return new object[0];
        }
        
        public void AdjustDispatcher(Action<StoredEvent> dispatcher)
        {
            _dispatcher = dispatcher;
        }

        public void AdjustClock(Func<DateTime> clock)
        {
            _clock = clock;
        }

        private StoredEvent[] getStoredEvents(object id, IEnumerable<object> events, long expectedInitialVersion)
        {
            var existing = _store.ContainsKey(id);

            if(existing == false && expectedInitialVersion != 1 && expectedInitialVersion != -1)
                throw new ConcurrencyException(id.ToString(), null, expectedInitialVersion);

            var existingMaxVersion = existing? _store[id].Max(x => x.Sequence) : 0;

            if(expectedInitialVersion != existingMaxVersion+1 && expectedInitialVersion != -1)
                throw new ConcurrencyException(id.ToString(), existingMaxVersion, expectedInitialVersion);
            
            int offset = 1;
            return events.Select(x =>
            {
                var version = existingMaxVersion + offset;
                offset++;
                return new StoredEvent(Guid.NewGuid(), id, version, _clock(), x);
            }).ToArray();
        }
    }
}