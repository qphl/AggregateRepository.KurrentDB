// <copyright file="TestEvent.cs" company="Cognisant">
// Copyright (c) Cognisant. All rights reserved.
// </copyright>

namespace CorshamScience.AggregateRepository.EventStore.Tests
{
    using System;

    internal class TestEvent
    {
        public TestEvent(Guid eventId) => EventId = eventId;

        public Guid EventId { get; }
    }
}
