using AggregateRepository.EventStore.Grpc;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using EventStore.Client;

namespace CorshamScience.AggregateRepository.EventStore.Grpc.Tests
{
    internal class EventStoreAggregateRepositoryTests : AggregateRepositoryTestFixture
    {
        private EventStoreClient? _client;

        protected async override Task InitRepositoryAsync()
        {
            var container = new TestcontainersBuilder<TestcontainersContainer>()
              .WithImage(new DockerImage("eventstore/eventstore:20.10.2-buster-slim"))
              .WithName("eventstore")
              .WithPortBinding(2113)
              .WithPortBinding(1113)
              .WithEnvironment(new Dictionary<string, string>()
              {
                  { "EVENTSTORE_CLUSTER_SIZE", "1" },
                  { "EVENTSTORE_RUN_PROJECTIONS", "All" },
                  { "EVENTSTORE_START_STANDARD_PROJECTIONS", "true" },
                  { "EVENTSTORE_EXT_TCP_PORT", "1113" },
                  { "EVENTSTORE_INSECURE", "true" },
                  { "EVENTSTORE_ENABLE_EXTERNAL_TCP", "true" },
                  { "EVENTSTORE_ENABLE_ATOM_PUB_OVER_HTTP", "true" }
              })
              .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(2113))
              .Build();

            await container.StartAsync();

            var settings = EventStoreClientSettings
            .Create("esdb://admin:changeit@127.0.0.1:2113?tls=false");

            _client = new EventStoreClient(settings);
            RepoUnderTest = new EventStoreAggregateRepository(_client);
        }

        protected override void CleanUpRepository()
        {
            _client?.Dispose();
        }        
    }
}
