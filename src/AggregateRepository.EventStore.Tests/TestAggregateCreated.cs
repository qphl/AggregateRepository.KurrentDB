// Copyright (c) Pharmaxo. All rights reserved.

namespace AggregateRepository.Kurrent.Tests;

internal class TestAggregateCreated
{
    public TestAggregateCreated(object aggregateId) => AggregateId = aggregateId;

    public object AggregateId { get; }
}
