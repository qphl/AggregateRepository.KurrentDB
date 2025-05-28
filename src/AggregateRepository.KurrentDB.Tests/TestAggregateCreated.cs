// Copyright (c) Pharmaxo. All rights reserved.

namespace AggregateRepository.KurrentDB.Tests;

internal class TestAggregateCreated
{
    public TestAggregateCreated(object aggregateId) => AggregateId = aggregateId;

    public object AggregateId { get; }
}
