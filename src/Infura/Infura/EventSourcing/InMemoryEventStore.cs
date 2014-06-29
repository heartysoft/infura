using System;
using System.Collections.Generic;

namespace Infura.EventSourcing
{
    public class InMemoryEventStore : EventStore
    {
        readonly Dictionary<object, List<object>> _store = new Dictionary<object, List<object>>();

        public void StoreEvents(object id, IEnumerable<object> events, long expectedInitialVersion)
        {
            if (_store.ContainsKey(id))
                _store[id].AddRange(events);
            else
                _store[id] = new List<object>(events);
        }

        public IEnumerable<object> LoadEvents(object id, long version = 0)
        {
            if (_store.ContainsKey(id))
                return _store[id];

            return new object[0];
        }
        
        public void AdjustDispatcher(Action<StoredEvent> dispatcher)
        {
            throw new NotImplementedException();
        }
    }
}