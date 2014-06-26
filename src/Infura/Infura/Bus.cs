using System;
using System.Collections.Generic;
using System.Linq;

namespace Infura
{
    public class Bus
    {
        readonly Dictionary<Type, List<Action<object>>> _handlers 
            = new Dictionary<Type, List<Action<object>>>();
        
        public void Register<T>(Action<T> handler)
        {
            Action<object> adjustedHandler = getAdjustedHandler(handler);

            var listForType = getListForMessageType<T>();
            listForType.Add(adjustedHandler);
        }

        public void ExecuteCommand(object command)
        {
            var commandType = command.GetType();
            _handlers[commandType].Single()(command);
        }

        public void Publish(object @event)
        {
            var eventType = @event.GetType();
            var assignableEventTypes = 
                _handlers.Keys.Where(x => x.IsAssignableFrom(eventType));
            
            foreach(var assignableEventType in assignableEventTypes)
                foreach (var handler in _handlers[assignableEventType])
                    handler(@event);
        }

        private Action<object> getAdjustedHandler<T>(Action<T> handler)
        {
            return x => handler((T) x);
        }

        private List<Action<object>> getListForMessageType<T>()
        {
            var type = typeof (T);

            if(_handlers.ContainsKey(type) == false)
                _handlers[type] = new List<Action<object>>();

            return _handlers[type];
        }
    }
}