using System;
using System.Linq;
using Infura.EventSourcing;

namespace Infura.InMemoryPublishing
{
    public class SlidingCacheDispatcher
    {
        readonly Action<StoredEvent> _handler;
        int _currentSlot;
        readonly object[] _cacheOfEventIds;

        public SlidingCacheDispatcher(int capacity, Action<StoredEvent> handler)
        {
            _handler = handler;
            _cacheOfEventIds = new object[capacity];
        }

        public void Publish(StoredEvent storedEvent)
        {
            if (_cacheOfEventIds.Contains(storedEvent.EventId))
                return;

            updateCache(storedEvent);
            _handler(storedEvent);
        }

        void updateCache(StoredEvent storedEvent)
        {
            _cacheOfEventIds[_currentSlot] = storedEvent.EventId;
            _currentSlot++;

            if (_currentSlot % _cacheOfEventIds.Length == 0)
                _currentSlot = 0;
        }
    }
}