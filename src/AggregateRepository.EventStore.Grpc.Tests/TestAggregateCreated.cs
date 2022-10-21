// <copyright file="TestAggregateCreated.cs" company="Cognisant">
// Copyright (c) Cognisant. All rights reserved.
// </copyright>

namespace CorshamScience.AggregateRepository.EventStore.Grpc.Tests
{
    internal class TestAggregateCreated
    {
        public TestAggregateCreated(object aggregateId) => AggregateId = aggregateId;

        public object AggregateId { get; }
    }
}
