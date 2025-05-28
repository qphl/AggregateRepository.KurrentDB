// Copyright (c) Pharmaxo. All rights reserved.

namespace AggregateRepository.KurrentDB.Tests;

internal class TestEvent
{
    public TestEvent(Guid eventId) => EventId = eventId;

    public Guid EventId { get; }
}
