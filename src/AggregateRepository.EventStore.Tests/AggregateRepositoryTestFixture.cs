// <copyright file="AggregateRepositoryTestFixture.cs" company="Corsham Science">
// Copyright (c) Corsham Science. All rights reserved.
// </copyright>

namespace CorshamScience.AggregateRepository.EventStore.Tests
{
    using CorshamScience.AggregateRepository.Core;
    using CorshamScience.AggregateRepository.Core.Exceptions;

    internal abstract class AggregateRepositoryTestFixture
    {
        private List<Guid> _storedEvents = new();
        private TestAggregate _retrievedAggregate = null!;
        private string _aggregateIdUnderTest = null!;

        protected IAggregateRepository RepoUnderTest { get; set; } = null!;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            await InitRepositoryAsync();            
        }

        [SetUp]
        public void SetUp()
        {
            _aggregateIdUnderTest = Guid.NewGuid().ToString();
            _storedEvents = new List<Guid>();
        }

        [OneTimeTearDown]
        public async Task OneTimeTearDown() => await CleanUpRepositoryAsync();

        [Test]
        public void Retreiving_an_aggregate_from_an_empty_eventstore_should_throw_an_exception() => Assert.Throws<AggregateNotFoundException>(() => RepoUnderTest.GetAggregateFromRepository<TestAggregate>(_aggregateIdUnderTest));

        [Test]
        public void Retreiving_a_nonexistant_aggregate_id_should_throw_an_exception()
        {
            var aggregate = new TestAggregate(_aggregateIdUnderTest);
            for (var i = 0; i < 2; i++)
            {
                var eventId = Guid.NewGuid();
                _storedEvents.Add(eventId);
                aggregate.GenerateEvent(eventId);
            }

            RepoUnderTest.Save(aggregate);
            Assert.Throws<AggregateNotFoundException>(() => RepoUnderTest.GetAggregateFromRepository<TestAggregate>(Guid.NewGuid().ToString()));
        }

        [Test]
        public void Retrieving_a_newly_created_aggregate_reconstructs_the_entity_correctly()
        {
            var aggregate = new TestAggregate(_aggregateIdUnderTest);
            RepoUnderTest.Save(aggregate);

            _retrievedAggregate = RepoUnderTest.GetAggregateFromRepository<TestAggregate>(_aggregateIdUnderTest);

            Assert.Multiple(() =>
            {
                Assert.That(_retrievedAggregate.Id, Is.EqualTo(_aggregateIdUnderTest));
                Assert.That(_retrievedAggregate.EventsApplied.Count, Is.EqualTo(0));
            });
        }

        [Test]
        public void Retrieving_an_aggregate_with_events_reconstructs_the_entity_correctly()
        {
            var aggregate = new TestAggregate(_aggregateIdUnderTest);
            for (var i = 0; i < 5; i++)
            {
                var eventId = Guid.NewGuid();
                _storedEvents.Add(eventId);
                aggregate.GenerateEvent(eventId);
            }

            RepoUnderTest.Save(aggregate);
            _retrievedAggregate = RepoUnderTest.GetAggregateFromRepository<TestAggregate>(_aggregateIdUnderTest);

            Assert.Multiple(() =>
            {
                Assert.That(_retrievedAggregate.Id, Is.EqualTo(_aggregateIdUnderTest));
                Assert.That(_retrievedAggregate.EventsApplied.Count, Is.EqualTo(_storedEvents.Count));
            });

            foreach (var id in _storedEvents)
            {
                Assert.That(_retrievedAggregate.EventsApplied, Does.Contain(id));
            }
        }

        [Test]
        public void Retrieving_an_aggregate_with_events_when_specifying_a_version_reconstructs_the_entity_correctly()
        {
            var aggregate = new TestAggregate(_aggregateIdUnderTest);
            for (var i = 0; i < 5; i++)
            {
                var eventId = Guid.NewGuid();
                _storedEvents.Add(eventId);
                aggregate.GenerateEvent(eventId);
            }

            RepoUnderTest.Save(aggregate);
            _retrievedAggregate = RepoUnderTest.GetAggregateFromRepository<TestAggregate>(_aggregateIdUnderTest, 6);

            Assert.Multiple(() =>
            {
                Assert.That(_retrievedAggregate.Id, Is.EqualTo(_aggregateIdUnderTest));
                Assert.That(_retrievedAggregate.EventsApplied.Count, Is.EqualTo(_storedEvents.Count));
            });

            foreach (var id in _storedEvents)
            {
                Assert.That(_retrievedAggregate.EventsApplied, Does.Contain(id));
            }
        }

        [Test]
        public void Retrieving_an_aggregate_with_events_when_specifying_an_incorrect_version_throws_aggregate_not_found_exception()
        {
            var aggregate = new TestAggregate(_aggregateIdUnderTest);
            for (var i = 0; i < 5; i++)
            {
                var eventId = Guid.NewGuid();
                _storedEvents.Add(eventId);
                aggregate.GenerateEvent(eventId);
            }

            RepoUnderTest.Save(aggregate);

            Assert.Throws<AggregateVersionException>(() => RepoUnderTest.GetAggregateFromRepository<TestAggregate>(_aggregateIdUnderTest, 10));
        }

        [Test]
        public void Retrieving_an_aggregate_with_events_reconstructs_the_entity_correctly_when_the_event_store_contains_multiple_aggregates()
        {
            var aggregate = new TestAggregate(_aggregateIdUnderTest);
            for (var i = 0; i < 5; i++)
            {
                var eventId = Guid.NewGuid();
                _storedEvents.Add(eventId);
                aggregate.GenerateEvent(eventId);
            }

            RepoUnderTest.Save(aggregate);
            var secondAggregate = new TestAggregate(Guid.NewGuid().ToString());
            for (var i = 0; i < 6; i++)
            {
                var eventId = Guid.NewGuid();
                secondAggregate.GenerateEvent(eventId);
            }

            RepoUnderTest.Save(secondAggregate);
            _retrievedAggregate = RepoUnderTest.GetAggregateFromRepository<TestAggregate>(_aggregateIdUnderTest);

            Assert.Multiple(() =>
            {
                Assert.That(_retrievedAggregate.Id, Is.EqualTo(_aggregateIdUnderTest));
                Assert.That(_retrievedAggregate.EventsApplied.Count, Is.EqualTo(_storedEvents.Count));
            });

            foreach (var id in _storedEvents)
            {
                Assert.That(_retrievedAggregate.EventsApplied, Does.Contain(id));
            }
        }

        [Test]
        public void Saving_new_events_to_an_existing_aggregate_should_correctly_persist_events()
        {
            var aggregate = new TestAggregate(_aggregateIdUnderTest);
            RepoUnderTest.Save(aggregate);

            _retrievedAggregate = RepoUnderTest.GetAggregateFromRepository<TestAggregate>(_aggregateIdUnderTest);

            var eventId = Guid.NewGuid();
            _retrievedAggregate.GenerateEvent(eventId);

            RepoUnderTest.Save(_retrievedAggregate);

            var actualAggregate = RepoUnderTest.GetAggregateFromRepository<TestAggregate>(_aggregateIdUnderTest);

            Assert.Multiple(() =>
            {
                Assert.That(actualAggregate.EventsApplied.Count, Is.EqualTo(1));
                Assert.That(actualAggregate.Id, Is.EqualTo(_aggregateIdUnderTest));
                Assert.That(actualAggregate.EventsApplied[0], Is.EqualTo(eventId));
                Assert.That(actualAggregate.Version, Is.EqualTo(2));
            });
        }

        [Test]
        public void Saving_an_aggregate_with_expected_version_less_than_the_actual_version_should_throw_a_concurrency_exception()
        {
            var aggregate = new TestAggregate(_aggregateIdUnderTest);
            for (var i = 0; i < 5; i++)
            {
                var eventId = Guid.NewGuid();
                _storedEvents.Add(eventId);
                aggregate.GenerateEvent(eventId);
            }

            RepoUnderTest.Save(aggregate);

            _retrievedAggregate = RepoUnderTest.GetAggregateFromRepository<TestAggregate>(_aggregateIdUnderTest, 3);

            for (var i = 0; i < 5; i++)
            {
                var eventId = Guid.NewGuid();
                _storedEvents.Add(eventId);
                _retrievedAggregate.GenerateEvent(eventId);
            }

            Assert.Throws<AggregateVersionException>(() => RepoUnderTest.Save(_retrievedAggregate));
        }

        [Test]
        public void Retrieving_an_aggregate_with_expected_version_greater_than_the_actual_version_should_throw_a_concurrency_exception()
        {
            var aggregate = new TestAggregate(_aggregateIdUnderTest);
            for (var i = 0; i < 5; i++)
            {
                var eventId = Guid.NewGuid();
                _storedEvents.Add(eventId);
                aggregate.GenerateEvent(eventId);
            }

            RepoUnderTest.Save(aggregate);
            Assert.Throws<AggregateVersionException>(() => RepoUnderTest.GetAggregateFromRepository<TestAggregate>(_aggregateIdUnderTest, 10));
        }

        protected abstract Task InitRepositoryAsync();

        protected abstract Task CleanUpRepositoryAsync();
    }
}
