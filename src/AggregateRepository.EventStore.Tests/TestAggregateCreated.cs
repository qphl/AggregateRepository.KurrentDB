// Copyright (c) Pharmaxo. All rights reserved.

namespace CorshamScience.AggregateRepository.EventStore.Tests;

internal class TestAggregateCreated
{
    public TestAggregateCreated(object aggregateId) => AggregateId = aggregateId;

    public object AggregateId { get; }
}
