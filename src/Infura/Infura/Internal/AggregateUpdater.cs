using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace Infura.Internal
{
    public static class AggregateUpdater
    {
        static readonly ConcurrentDictionary<
            Tuple<Type, Type>, 
            Action<Aggregate, object>> Cache 
                = new ConcurrentDictionary<
                    Tuple<Type, Type>, 
                    Action<Aggregate, object>>();

        public static void Update(Aggregate instance, object @event)
        {
            var eventType = @event.GetType();
            var aggregateType = instance.GetType();
            var tuple = new Tuple<Type, Type>(aggregateType, eventType);
            var action = Cache.GetOrAdd(tuple, ActionFactory);

            action(instance, @event);
        }

        static Action<Aggregate, object> ActionFactory(Tuple<Type, Type> key)
        {
            var eventType = key.Item2;
            var aggregateType = key.Item1;

            const string methodName = "UpdateFrom";
            var method = aggregateType
                .GetMethods(BindingFlags.NonPublic
                    | BindingFlags.Instance)
                .SingleOrDefault(x => x.Name == methodName
                    && x.GetParameters()
                                     .Single()
                                     .ParameterType.IsAssignableFrom(eventType));

            if(method == null) return (x, y) => { };
 
            return (instance, @event) => 
                method.Invoke(instance, new[] { @event });
        }
    }
}