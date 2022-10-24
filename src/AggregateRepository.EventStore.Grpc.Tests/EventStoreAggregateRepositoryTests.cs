using AggregateRepository.EventStore.Grpc;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using EventStore.Client;

namespace CorshamScience.AggregateRepository.EventStore.Grpc.Tests
{
    internal class EventStoreAggregateRepositoryTests : AggregateRepositoryTestFixture
    {
        protected async override Task InitRepositoryAsync()
        {
            var container = new TestcontainersBuilder<TestcontainersContainer>()
              .WithImage(new DockerImage("eventstore/eventstore:release-4.1.1"))
              .WithName("eventstore411")
              .WithPortBinding(2113)
              .WithPortBinding(1113)
              .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(2113))
              .Build();
           
            await container.StartAsync();

            var settings = EventStoreClientSettings
            .Create("esdb://admin:changeit@127.0.0.1:2113?tls=false");

            using var client = new EventStoreClient(settings);
            RepoUnderTest = new EventStoreAggregateRepository(client);
        }

        protected async override Task CleanUpRepositoryAsync()
        {
            //await _container.DisposeAsync();
        }        
    }
}
