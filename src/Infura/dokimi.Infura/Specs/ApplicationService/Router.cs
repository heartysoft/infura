using System;
using System.Collections.Generic;

namespace dokimi.core.Specs.ApplicationService
{
    public class Router
    {
        readonly Dictionary<Type, List<Action<object>>> _handlers = new Dictionary<Type, List<Action<object>>>();
        private Router()
        {
        }

        public static Router Create()
        {
            return new Router();
        }

        public Router Route<T>(Action<T> handler)
        {
            var list = getList(typeof(T));
            list.Add(x => handler((T)x));
            
            return this;
        }

        public void Handle(object message)
        {
            var list = getList(message.GetType());

            foreach (var action in list)
                action(message);
        }


        private List<Action<object>> getList(Type type)
        {
            if(_handlers.ContainsKey(type) == false)
                _handlers[type] = new List<Action<object>>();

            return _handlers[type];
        }
    }
}