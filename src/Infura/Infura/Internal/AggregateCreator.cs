using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace Infura.Internal
{
    public static class AggregateCreator
    {
        static readonly ConcurrentDictionary<Type, Func<object, object>> Cache
            = new ConcurrentDictionary<Type, Func<object, object>>();

        static readonly Action<object, object> _idSetter;
        const string IdFieldName = "_id";

        static AggregateCreator()
        {
            var setter = typeof(Aggregate).GetField(
                IdFieldName, BindingFlags.Instance | BindingFlags.NonPublic);
            _idSetter = (aggregate, id) => setter.SetValue(aggregate, id);
        }

        public static T CreateEmptyInstanceWithId<T>(object id) where T:Aggregate
        {
            var instance = Cache.GetOrAdd(
                typeof (T), 
                InitializerFactory)(id) as T;

            return instance;
        }

        static Func<object, object> InitializerFactory(Type type)
        {
            var ctor = type.GetConstructor(
                BindingFlags.Instance | BindingFlags.NonPublic,
                null,
                new Type[] { },
                null);

            return id =>
            {
                var instance = ctor.Invoke(null);
                _idSetter(instance, id);
                return instance;
            };
        }
    }
}