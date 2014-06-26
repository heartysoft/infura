using System;
using Infura.EventSourcing;

namespace Infura.InMemoryPublishing
{
    public class RetryingDispatcher
    {
        readonly Action<StoredEvent> _downstreamDispatcher;
        readonly Action<StoredEvent> _errorHandler;
        readonly int _maxRetries;

        public RetryingDispatcher(Action<StoredEvent> downstreamDispatcher, 
            Action<StoredEvent> errorHandler, 
            int maxRetries)
        {
            _downstreamDispatcher = downstreamDispatcher;
            _errorHandler = errorHandler;
            _maxRetries = maxRetries;
        }

        public void Dispatch(StoredEvent storedEvent)
        {
            int tryCount = 0;

            while(true)
            {
                try
                {
                    _downstreamDispatcher(storedEvent);
                    break;
                }
                catch(Exception)
                {
                    if (tryCount == _maxRetries)
                    {
                        _errorHandler(storedEvent);
                        throw;
                    }

                    ++tryCount;
                }
            }
        }
    }
}