// <copyright file="EventStoreAggregateRepositoryTests.cs" company="Corsham Science">
// Copyright (c) Corsham Science. All rights reserved.
// </copyright>

namespace CorshamScience.AggregateRepository.EventStore.Tests
{
    using CorshamScience.AggregateRepository.EventStore;
    using DotNet.Testcontainers.Builders;
    using System.Runtime.InteropServices;
    using DotNet.Testcontainers.Images;
    using DotNet.Testcontainers.Containers;
    using global::EventStore.Client;

    internal class EventStoreAggregateRepositoryTests : AggregateRepositoryTestFixture
    {
        private IContainer? _container;
        private EventStoreClient? _client;

        protected override async Task InitRepository()
        {
            const string eventStoreVersion = "22.10.1";
            string imageName = RuntimeInformation.OSArchitecture == Architecture.Arm64
                // if on arm (like an m1 mac) use the alpha arm image from github
                ? $"ghcr.io/eventstore/eventstore:22.10.1-alpha-arm64v8"
                : $"eventstore/eventstore:{eventStoreVersion}-buster-slim";
            
            const int hostPort = 2113;
            
            _container = new ContainerBuilder()
              .WithImage(new DockerImage(imageName))
              .WithCleanUp(true)
              .WithPortBinding(hostPort)
              .WithEnvironment(new Dictionary<string, string>
              {
                  { "EVENTSTORE_INSECURE", "true" },
                  { "EVENTSTORE_ENABLE_ATOM_PUB_OVER_HTTP", "true" },
              })
              .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(hostPort))
              .Build();

            await _container.StartAsync();

            var settings = EventStoreClientSettings
                .Create($"esdb://admin:changeit@127.0.0.1:{hostPort}?tls=false");

            _client = new EventStoreClient(settings);
            RepoUnderTest = new EventStoreAggregateRepository(_client);
        }

        protected override async Task CleanUpRepository()
        {
            if (_container != null)
            {
                await _container.DisposeAsync();
            }

            if (_client != null)
            {
                await _client.DisposeAsync();
            }
        }
    }
}
