using System;
using Machine.Specifications;

namespace Infura.Tests.Domain
{
    public class AggregateSubjectAttribute : SubjectAttribute
    {
        public AggregateSubjectAttribute() : base("Aggregate")
        {
        }
    }

    public class SomeAggregate : Aggregate
    {
        string _someValue;
        public string SomeValueForTestPurposes { get { return _someValue; } }
        bool _hasDoneSomething;
        public bool HasDoneSomething { get { return _hasDoneSomething; } }

        private SomeAggregate()
        {
        }

        public SomeAggregate(Guid id, string someValue)
            : base(id)
        {
            var @event = new SomeAggregateCreatedEvent(id, someValue);
            Apply(@event);
        }

        private void UpdateFrom(SomeAggregateCreatedEvent @event)
        {
            _someValue = @event.SomeValue;
        }

        public void DoSomeThing()
        {
            var @event = new SomeAggregateDidSomethingEvent((Guid)this.Id);
            Apply(@event);
        }

        private void UpdateFrom(SomeAggregateDidSomethingEvent @event)
        {
            _hasDoneSomething = true;
        }
    }

    class SomeAggregateCreatedEvent
    {
        public Guid Id { get; private set; }
        public string SomeValue { get; private set; }

        public SomeAggregateCreatedEvent(Guid id, string someValue)
        {
            Id = id;
            SomeValue = someValue;
        }
    }

    class SomeAggregateDidSomethingEvent
    {
        public Guid Id { get; private set; }

        public SomeAggregateDidSomethingEvent(Guid id)
        {
            Id = id;
        }
    }

    [AggregateSubject]
    public class when_creating_an_aggregate
    {
        static SomeAggregate aggregate;
        static Guid id;

        Establish context = () => id = Guid.NewGuid();

        Because of = () => aggregate = new SomeAggregate(id, "foo");

        It should_have_given_id = () => aggregate.Id.ShouldEqual(id);
        It should_have_expected_some_value = () => 
            aggregate.SomeValueForTestPurposes.ShouldEqual("foo");
    }

    [AggregateSubject]
    public class when_events_are_generated
    {
        static SomeAggregate aggregate;
        static object[] events;

        Establish context = () =>
        {
            aggregate = new SomeAggregate(Guid.NewGuid(), "foo");
            aggregate.DoSomeThing();
        };

        Because of = () => events = aggregate.GetUncommittedEvents();

        It should_have_those_events_in_sequence = () =>
        {
            events[0].ShouldBeOfExactType<SomeAggregateCreatedEvent>();
            events[1].ShouldBeOfExactType<SomeAggregateDidSomethingEvent>();
        };

        It should_have_expected_version = () => aggregate.Version.ShouldEqual(2);
    }

    [AggregateSubject]
    public class when_initializing_an_empty_aggregate_from_history
    {
        static SomeAggregate aggregate;
        static Guid id;
        static object[] events;

        Establish context = () =>
        {
            id = Guid.NewGuid();

            events = new object[]
            {
                new SomeAggregateCreatedEvent(id, "foo"),
                new SomeAggregateDidSomethingEvent(id)
            };
        };

        Because of = () => 
            aggregate = Aggregate.InitializeFromHistory<SomeAggregate>(id, events);

        It should_hydrate_the_aggregate_state = () =>
        {
            aggregate.Id.ShouldEqual(id);
            aggregate.SomeValueForTestPurposes.ShouldEqual("foo");
            aggregate.HasDoneSomething.ShouldBeTrue();
            aggregate.Version.ShouldEqual(2);
        };
    }

    [AggregateSubject]
    public class when_uncommitted_changes_are_cleared
    {
        static SomeAggregate aggregate;

        Establish context = () =>
        {
            aggregate = new SomeAggregate(Guid.NewGuid(), "something");
            aggregate.DoSomeThing();
        };

        Because of = () => aggregate.ClearUncommittedChanges();

        It should_have_no_uncommitted_changes = () =>
            aggregate.GetUncommittedEvents().Length.ShouldEqual(0);
    }
}