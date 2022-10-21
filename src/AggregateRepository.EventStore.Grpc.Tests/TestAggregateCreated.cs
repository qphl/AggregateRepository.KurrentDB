namespace CorshamScience.AggregateRepository.EventStore.Grpc.Tests
{
    internal class TestAggregateCreated
    {
        public TestAggregateCreated(object aggregateId) => AggregateId = aggregateId;

        public object AggregateId { get; }
    }
}
