// <copyright file="EventStoreAggregateRepositoryTests.cs" company="Cognisant">
// Copyright (c) Cognisant. All rights reserved.
// </copyright>

namespace AggregateRepository.EventStore.Tests
{
    using CR.AggregateRepository.EventStore;
    using global::EventStore.ClientAPI;
    using global::EventStore.ClientAPI.Embedded;

    public class EventStoreAggregateRepositoryTests : AggregateRepositoryTestFixture
    {
        private IEventStoreConnection _connection;
        private EmbeddedEventStore _eventStore;

        protected override void InitRepository()
        {
            _eventStore = new EmbeddedEventStore(11113, 12113);
            _eventStore.Start();

            _connection = EmbeddedEventStoreConnection.Create(_eventStore.Node);
            _connection.ConnectAsync().Wait();

            RepoUnderTest = new EventStoreAggregateRepository(_connection);
        }

        protected override void CleanUpRepository()
        {
            _connection.Close();
            _eventStore.Stop();
        }
    }
}
