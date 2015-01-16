using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infura
{
    public class Bus
    {
        readonly Dictionary<Type, List<Func<object, Task>>> _handlers = new Dictionary<Type, List<Func<object, Task>>>();

        public Bus Register<T>(Func<T, Task> handler)
        {
            Func<object, Task> adjustedHandler = getAdjustedHandler(handler);

            var listForType = getListForMessageType<T>();
            listForType.Add(adjustedHandler);
            return this;
        }

        public async Task ExecuteCommand(object command)
        {
            var commandType = command.GetType();
            await _handlers[commandType].Single()(command);
        }

        public async Task Publish(object @event)
        {
            var eventType = @event.GetType();
            var assignableEventTypes =
                _handlers.Keys.Where(x => x.IsAssignableFrom(eventType));

            List<Task> tasks = new List<Task>();

            foreach (var assignableEventType in assignableEventTypes)
                foreach (var handler in _handlers[assignableEventType])
                    tasks.Add(handler(@event));

            await Task.WhenAll(tasks);
        }

        private Func<object, Task> getAdjustedHandler<T>(Func<T, Task> handler)
        {
            return x => handler((T)x);
        }

        private List<Func<object, Task>> getListForMessageType<T>()
        {
            var type = typeof(T);

            if (_handlers.ContainsKey(type) == false)
                _handlers[type] = new List<Func<object, Task>>();

            return _handlers[type];
        }
    }
}