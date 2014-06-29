using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infura.EventSourcing;
using Res.Client;

namespace Infura.ResClient
{
    public class ResClientEventStore : EventStore
    {
        private Action<StoredEvent> _dispatcher = _ => { };
        Func<DateTime> _clock = () => DateTime.UtcNow;
        private readonly string _context;
        private readonly ResClientEventPublisher _publisher;
        private readonly ResQueryClient _queryClient;
        private readonly Func<string, string, object> _deserialiser;

        public ResClientEventStore(string context, ResClientEventPublisher publisher, ResQueryClient queryClient, Func<string, string, object> deserialiser)
        {
            _context = context;
            _publisher = publisher;
            _queryClient = queryClient;
            _deserialiser = deserialiser;
        }

        public void StoreEvents(object id,
           IEnumerable<object> events,
           long expectedInitialVersion)
        {
            var eventList = events.ToList();
            var streamId = id.ToString();


            int offset = 0;
            var storedEvents = eventList.Select(
                x =>
                {
                    ++offset;
                    return new StoredEvent(Guid.NewGuid(), streamId, expectedInitialVersion + offset - 1, _clock(), x);
                }
                ).ToArray();

            var eventsToStore = storedEvents.Select(
                    x => new EventObject((Guid)x.EventId, null, x.EventData, x.Timestamp))
                    .ToArray();

            try
            {
                _publisher
                    .Publish(id.ToString(), eventsToStore, expectedInitialVersion)
                    .GetAwaiter().GetResult();
            }
            catch (Res.Client.Exceptions.ConcurrencyException ex)
            {
                throw new ConcurrencyException(streamId,
                    null,
                    expectedInitialVersion,
                    ex);
            }

            dispatchEvents(storedEvents);
        }

        void dispatchEvents(IEnumerable<StoredEvent> events)
        {
            foreach (var e in events)
                _dispatcher(e);
        }

        public IEnumerable<object> LoadEvents(object id, long version = 0)
        {
            var events = _queryClient.LoadEvents(_context, id.ToString(), version, null, null)
                .GetAwaiter().GetResult();
            return events.Events.Select(x => _deserialiser(x.TypeTag, x.Body));
        }
        
        public void AdjustDispatcher(Action<StoredEvent> dispatcher)
        {
            _dispatcher = dispatcher;
        }
        public void AdjustClock(Func<DateTime> clock)
        {
            _clock = clock;
        }
    }
}
