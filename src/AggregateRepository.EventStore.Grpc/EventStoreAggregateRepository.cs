// <copyright file="EventStoreAggregateRepository.cs" company="Corsham Science">
// Copyright (c) Corsham Science. All rights reserved.
// </copyright>

namespace AggregateRepository.EventStore
{
    using CorshamScience.AggregateRepository.Core;
    using CorshamScience.AggregateRepository.Core.Exceptions;
    using global::EventStore.Client;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.Text;

    /// <inheritdoc />
    /// <summary>
    /// Implementation of <see cref="T:CorshamScience.AggregateRepository.Core.IAggregateRepository" /> which uses Event Store as underlying storage for an aggregate's events.
    /// </summary>
    public class EventStoreAggregateRepository : IAggregateRepository
    {
        private readonly EventStoreClient _eventStoreClient;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventStoreAggregateRepository"/> class using the provided <see cref="IEventStoreConnection"/> to store and retrieve events for an <see cref="IAggregate"/>.
        /// </summary>
        /// <param name="connection">The <see cref="IEventStoreConnection"/> to read from and write to.</param>
        public EventStoreAggregateRepository(EventStoreClient eventStoreClient) => _eventStoreClient = eventStoreClient;

        /// <inheritdoc />
        /// <exception cref="AggregateNotFoundException">
        /// Thrown when the provided <see cref="IAggregate"/>'s ID matches a deleted stream in the EventStore the <see cref="IEventStoreConnection"/> is configured to use.
        /// </exception>
        public void Save(IAggregate aggregateToSave)
        {
            var events = aggregateToSave.GetUncommittedEvents().Cast<object>().ToList();
            var streamName = StreamNameForAggregateId(aggregateToSave.Id);

            var originalVersion = aggregateToSave.Version - events.Count;
            ulong expectedVersion = originalVersion == 0 ? expectedVersion = StreamRevision.None : (ulong)(originalVersion - 1);

            var preparedEvents = events
                .Select(ToEventData)
                .ToArray();

            try
            {
                _eventStoreClient.AppendToStreamAsync(streamName, expectedVersion, preparedEvents).Wait();
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

        /// <inheritdoc />
        public T GetAggregateFromRepository<T>(object aggregateId, int version = int.MaxValue)
            where T : IAggregate
        {
            if (version <= 0)
            {
                throw new InvalidOperationException("Cannot get version <= 0");
            }

            var eventsCount = 0;
            var streamName = StreamNameForAggregateId(aggregateId);
            var aggregate = (T)Activator.CreateInstance(typeof(T), true)!;

            try
            {
                var readResult = _eventStoreClient.ReadStreamAsync(
                    Direction.Forwards,
                    streamName,
                    StreamPosition.Start,
                    version);

                if (readResult.ReadState.Result == ReadState.StreamNotFound)
                {
                    throw new StreamNotFoundException(streamName);
                }

                var events = readResult.ToListAsync().Result;

                foreach (var @event in events)
                {
                    aggregate.ApplyEvent(Deserialize(@event));
                }

                eventsCount += events.Count;
            }
            catch (StreamNotFoundException e)
            {
                throw new AggregateNotFoundException(e.Message, e);
            }

            // if version is greater than number of events, throw exception
            if (eventsCount < version && version != int.MaxValue)
            {
                throw new AggregateVersionException("version is higher than actual version");
            }

            return aggregate;
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
            const string metaDataPropertyName = "ClrType";

            var jsonData = Encoding.UTF8.GetString(resolvedEvent.Event.Data.Span);
            var metaData = Encoding.UTF8.GetString(resolvedEvent.Event.Metadata.Span);
            var eventClrTypeName = JObject.Parse(metaData).Property(metaDataPropertyName)?.Value;

            if (eventClrTypeName is null)
            {
                throw new InvalidOperationException($"Event Metadata has no property '{metaDataPropertyName}'");
            }

            return JsonConvert.DeserializeObject(jsonData, Type.GetType((string)eventClrTypeName));
        }

        private static string StreamNameForAggregateId(object id) => "aggregate-" + id;
    }
}
