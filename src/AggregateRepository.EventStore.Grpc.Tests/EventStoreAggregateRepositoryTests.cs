using AggregateRepository.EventStore.Grpc;
using Docker.DotNet.Models;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using EventStore.Client;

namespace CorshamScience.AggregateRepository.EventStore.Grpc.Tests
{
    internal class EventStoreAggregateRepositoryTests : AggregateRepositoryTestFixture
    {
        protected async override Task InitRepositoryAsync()
        {
            var testcontainersBuilder = new TestcontainersBuilder<TestcontainersContainer>()
              .WithImage("eventstore/eventstore:release-4.1.1")
              .WithName("eventstore411")
              .WithPortBinding(9113)
              .WithWaitStrategy(Wait.ForWindowsContainer().UntilPortIsAvailable(9113));

            await using var testcontainers = testcontainersBuilder.Build();
            await testcontainers.StartAsync();
            //testcontainers.GetMappedPublicPort();

            var settings = EventStoreClientSettings
                .Create("esdb://admin:changeit@127.0.0.1:9113?tls=false");

            using var client = new EventStoreClient(settings);
            RepoUnderTest = new EventStoreAggregateRepository(client);
        }

        protected override void CleanUpRepository()
        {
            
        }        
    }
}
