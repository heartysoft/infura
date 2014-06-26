using System;
using System.Collections.Generic;

namespace Infura.EventSourcing
{
    public interface EventStore
    {
        void StoreEvents(
            object id, 
            IEnumerable<object> events, 
            long expectedInitialVersion);
        
        IEnumerable<object> LoadEvents(object id, long version = 0);
        StoredEvent[] GetEventsSince(DateTime timestamp, int maxCount = 1000);
        void AdjustDispatcher(Action<StoredEvent> dispatcher);
    }
}