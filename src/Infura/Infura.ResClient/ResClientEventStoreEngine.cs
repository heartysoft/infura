using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Res.Client;

namespace Infura.ResClient
{
    public class ResClientEventStoreEngine : IDisposable
    {
        private readonly ResPublishEngine _resPublishEngine;
        private readonly ResQueryEngine _resQueryEngine;

        public ResClientEventStoreEngine(string resPublishEndpoint, string resQueryEndpoint)
        {
            _resPublishEngine = new ResPublishEngine(resPublishEndpoint, false);
            _resQueryEngine = new ResQueryEngine(resQueryEndpoint);
        }

        public ResClientEventStore Create(string context, TypeTagResolver typeResolver, TimeSpan publishDefaultTimeout, TimeSpan queryDefaultTimeout)
        {
            var jsonSerialiserSettings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
            Func<object, string> serialiser = x => JsonConvert.SerializeObject(x, jsonSerialiserSettings);
            var publisher = _resPublishEngine.CreateEventPublisher(context, publishDefaultTimeout, typeResolver, serialiser);
            var queryClient = _resQueryEngine.CreateClient(queryDefaultTimeout);
            Func<string, string, object> deserialiser =
                (tag, str) => JsonConvert.DeserializeObject(str, typeResolver.GetTypeFor(tag), jsonSerialiserSettings);
            var eventStore = new ResClientEventStore(context, publisher, queryClient, deserialiser);

            return eventStore;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;

            _resQueryEngine.Dispose();
            _resPublishEngine.Dispose();
        }
    }
}