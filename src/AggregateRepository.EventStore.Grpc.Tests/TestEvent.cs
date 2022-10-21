namespace CorshamScience.AggregateRepository.EventStore.Grpc.Tests
{
    using System;

    internal class TestEvent
    {
        public TestEvent(Guid eventId) => EventId = eventId;

        public Guid EventId { get; }
    }
}
