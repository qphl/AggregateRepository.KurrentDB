// Copyright (c) Pharmaxo. All rights reserved.

using System.Runtime.InteropServices;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;
using KurrentDB.Client;
using PharmaxoScientific.AggregateRepository.KurrentDB;

namespace AggregateRepository.KurrentDB.Tests;

internal class KurrentDbAggregateRepositoryTests : AggregateRepositoryTestFixture
{
    private IContainer? _container;
    private KurrentDBClient? _client;

    protected override async Task InitRepository()
    {
        const string eventStoreVersion = "24.10.5";

        var imageName = RuntimeInformation.OSArchitecture == Architecture.Arm64
            // if on arm (like an m1 mac) use the alpha arm image from github
            ? $"ghcr.io/eventstore/eventstore:{eventStoreVersion}-alpha-arm64v8"
            : $"eventstore/eventstore:{eventStoreVersion}-bookworm-slim";

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
        RepoUnderTest = new KurrentDbAggregateRepository(_client);
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
