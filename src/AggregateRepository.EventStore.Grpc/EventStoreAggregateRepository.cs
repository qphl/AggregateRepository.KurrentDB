using CorshamScience.AggregateRepository.Core;
using CorshamScience.AggregateRepository.Core.Exceptions;
using EventStore.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace AggregateRepository.EventStore.Grpc
{
    public class EventStoreAggregateRepository : IAggregateRepository
    {
        private readonly EventStoreClient _eventStoreClient;

        public EventStoreAggregateRepository(EventStoreClient eventStoreClient)
        {
            _eventStoreClient = eventStoreClient;
        }

        public void Save(IAggregate aggregateToSave)
        {
            var events = aggregateToSave.GetUncommittedEvents().Cast<object>().ToList();
            var streamName = StreamNameForAggregateId(aggregateToSave.Id);

            var originalVersion = aggregateToSave.Version - events.Count;

            var preparedEvents = events
                .Select(ToEventData)
                .ToArray();

            var readResult = _eventStoreClient.ReadStreamAsync(
                Direction.Forwards,
                streamName,
                StreamPosition.Start);

            var lastRevision = readResult.LastAsync().Result.Event.EventNumber.ToUInt64();
            
            try
            {
                _eventStoreClient.AppendToStreamAsync(streamName, lastRevision, preparedEvents).Wait();
                aggregateToSave.ClearUncommittedEvents();
            }
            catch (StreamDeletedException ex)
            {
                throw new AggregateNotFoundException("Aggregate not found, stream deleted", ex);
            }
            catch (AggregateException ex)
            {
                var exceptions = ex.InnerExceptions;
                if (exceptions.Count == 1 && exceptions[0].GetType() == typeof(WrongExpectedVersionException))
                {
                    throw new AggregateVersionException("Aggregate version incorrect", ex);
                }

                throw;
            }
        }

        public T GetAggregateFromRepository<T>(object aggregateId, int version = 2147483647)
            where T : IAggregate
        {
            if (version <= 0)
            {
                throw new InvalidOperationException("Cannot get version <= 0");
            }

            var streamName = StreamNameForAggregateId(aggregateId);
            var instance = (T)Activator.CreateInstance(typeof(T), true)!;

            try
            {
                var readResult = _eventStoreClient.ReadStreamAsync(
                    Direction.Forwards,
                    streamName,
                    StreamPosition.Start);

                var events = readResult.ToListAsync().Result;

                foreach (var @event in events)
                {
                    instance.ApplyEvent(Deserialize(@event));
                }
            }
            catch (StreamNotFoundException)
            {
                throw new AggregateNotFoundException();
            }

            return instance;
        }

        private static EventData ToEventData(object @event)
        {
            var data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(@event));

            var eventHeaders = new
            {
                ClrType = @event.GetType().AssemblyQualifiedName,
            };

            var metadata = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(eventHeaders));
            var typeName = @event.GetType().Name;

            return new(
                Uuid.NewUuid(),
                typeName,
                data,
                metadata);
        }

        private static object Deserialize(ResolvedEvent resolvedEvent)
        {
            var jsonData = Encoding.UTF8.GetString(resolvedEvent.Event.Data.Span);
            var metaData = Encoding.UTF8.GetString(resolvedEvent.Event.Metadata.Span);
            var eventClrTypeName = JObject.Parse(metaData).Property("ClrType")?.Value;

            if (eventClrTypeName == null)
            {
                throw new Exception("Event metadata has no property ClrType");
            }

            return JsonConvert.DeserializeObject(jsonData, Type.GetType((string)eventClrTypeName));
        }

        private static string StreamNameForAggregateId(object id) => "aggregate-" + id;
    }
}
