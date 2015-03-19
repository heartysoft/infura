using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        public Router SyncRoute<T>(Action<T> handler)
        {
            var list = getList(typeof(T));
            list.Add(x => handler((T)x));
            
            return this;
        }

	    public Router Route<T>(Func<T, Task> handler)
	    {
			var list = getList(typeof(T));
			list.Add(x => handler((T)x).Wait());

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