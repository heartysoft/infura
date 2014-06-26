using System.Collections.Generic;
using Infura.Internal;

namespace Infura
{
    public abstract class Aggregate
    {
        private readonly object _id;
        public long Version { get; private set; }
        readonly List<object> _events = new List<object>();

        public  object Id
        {
            get { return _id; }
        }

        protected Aggregate()
        {
        }
        
        protected Aggregate(object id)
        {
            _id = id;
        }

        public object[] GetUncommittedEvents()
        {
            return _events.ToArray();
        }

        public void ClearUncommittedChanges()
        {
            _events.Clear();
        }

        protected void Apply(object @event)
        {
            _events.Add(@event);
            UpdateInstanceWithEvent(@event);
        }


        public static T InitializeFromHistory<T>(object id, object[] events) 
            where T: Aggregate
        {
            var instance = AggregateCreator.CreateEmptyInstanceWithId<T>(id);

            foreach (var @event in events)
                instance.UpdateInstanceWithEvent(@event);

            return instance;
        }

        void UpdateInstanceWithEvent(object @event)
        {
            AggregateUpdater.Update(this, @event);
            Version++;
        }
    }
}