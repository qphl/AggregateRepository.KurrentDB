// Copyright (c) Pharmaxo. All rights reserved.

using System.Runtime.InteropServices;
using CorshamScience.AggregateRepository.EventStore;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using KurrentDB.Client;

namespace AggregateRepository.EventStore.Tests;

internal class EventStoreAggregateRepositoryTests : AggregateRepositoryTestFixture
{
    private IContainer? _container;
    private KurrentDBClient? _client;

    protected override async Task InitRepository()
    {
        const string eventStoreVersion = "24.10.5";

        // If on arm (like an m1 mac) use the alpha arm image from github
        var architectureSuffix = RuntimeInformation.OSArchitecture == Architecture.Arm64
            ? "-alpha-arm64v8"
            : "-bookworm-slim";

        var imageName = $"eventstore/eventstore:{eventStoreVersion}{architectureSuffix}";

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

        var settings = KurrentDBClientSettings
            .Create($"esdb://admin:changeit@127.0.0.1:{hostPort}?tls=false");

        _client = new KurrentDBClient(settings);
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
