// Copyright (c) Pharmaxo. All rights reserved.

namespace AggregateRepository.Kurrent.Tests;

internal class TestEvent
{
    public TestEvent(Guid eventId) => EventId = eventId;

    public Guid EventId { get; }
}
