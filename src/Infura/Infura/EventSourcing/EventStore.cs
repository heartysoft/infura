using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Infura.EventSourcing
{
    public interface EventStore
    {
        Task StoreEvents(
            object id, 
            IEnumerable<object> events, 
            long expectedInitialVersion);
        
        Task<IEnumerable<object>> LoadEvents(object id, long version = 0);
        void AdjustDispatcher(Action<StoredEvent> dispatcher);
    }
}