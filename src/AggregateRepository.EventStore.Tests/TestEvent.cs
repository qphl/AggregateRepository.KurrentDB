// Copyright (c) Pharmaxo. All rights reserved.

using System;

namespace CorshamScience.AggregateRepository.EventStore.Tests;
internal class TestEvent
{
    public TestEvent(Guid eventId) => EventId = eventId;

    public Guid EventId { get; }
}
