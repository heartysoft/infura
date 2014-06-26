using System;

namespace Infura.EventSourcing
{
    public class StoredEvent
    {
        public object EventId { get; private set; }
        public object StreamId { get; private set; }
        public long Sequence { get; private set; }
        public DateTime Timestamp { get; private set; }
        public object EventData { get; private set; }

        public StoredEvent(
            object eventId,
            object streamId, 
            long sequence, 
            DateTime timestamp,
            object eventData)
        {
            EventId = eventId;
            StreamId = streamId;
            Sequence = sequence;
            Timestamp = timestamp;
            EventData = eventData;
        }
    }
}