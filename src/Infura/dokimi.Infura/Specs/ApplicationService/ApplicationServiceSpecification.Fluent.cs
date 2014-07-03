using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Security.Cryptography.X509Certificates;
using Infura;
using Infura.EventSourcing;
using MessageCount = dokimi.core.Specs.ApplicationService.MessageCount;
using Router = dokimi.core.Specs.ApplicationService.Router;

namespace dokimi.core.Specs.ApplicationService
{
    public partial class ApplicationServiceSpecification
    {
        public class GivenContext 
        {
            private readonly ApplicationServiceSpecification _instance;
            private readonly List<GivenEvent> _history = new List<GivenEvent>();

            public GivenContext(ApplicationServiceSpecification instance)
            {
                _instance = instance;
            }

            public ExpectingEvents ForStream(object id)
            {
                return new ExpectingEvents(id, this);
            }

            public ExpectingThen When(string description, object message)
            {
                return new ExpectingWhen(_instance).When(description, message);
            }

            public ExpectingThen When(object message)
            {
                return new ExpectingWhen(_instance).When(message);
            }
            
            public class ExpectingEvents
            {
                private readonly object _streamId;
                private readonly GivenContext _givenContext;

                public ExpectingEvents(object streamId, GivenContext givenContext)
                {
                    _streamId = streamId;
                    _givenContext = givenContext;
                }

                public ExpectingAnotherStreamEventOrWhen Events(params object[] events)
                {
                    return new ExpectingAnotherStreamEventOrWhen(_streamId, _givenContext)
                        .Events(events);
                }

                public ExpectingAnotherStreamEventOrWhen Event(string description, object @event)
                {
                    return new ExpectingAnotherStreamEventOrWhen(_streamId, _givenContext)
                        .Event(description, @event);
                }

                public ExpectingAnotherStreamEventOrWhen Event(object @event)
                {
                    return Event(null, @event);
                }

                public class ExpectingAnotherStreamEventOrWhen
                {
                    private readonly object _streamId;
                    private readonly GivenContext _given;

                    public ExpectingAnotherStreamEventOrWhen(object streamId, GivenContext given)
                    {
                        _streamId = streamId;
                        _given = given;
                    }

                    public ExpectingThen When(string description, object message)
                    {
                        return _given.When(description, message);
                    }

                    public ExpectingThen When(object message)
                    {
                        return _given.When(message);
                    }

                    public ExpectingAnotherStreamEventOrWhen Events(params object[] events)
                    {
                        _given._history.AddRange(events.Select(x => new GivenEvent(_streamId, x, null)));
                        return this;
                    }

                    public ExpectingAnotherStreamEventOrWhen Event(string description, object @event)
                    {
                        _given._history.Add(new GivenEvent(_streamId, @event, description));
                        return this;
                    }

                    public ExpectingAnotherStreamEventOrWhen Event(object @event)
                    {
                        return Event(null, @event);
                    }

                    public ExpectingEvents ForAnotherStream(object streamId)
                    {
                        return _given.ForStream(streamId);
                    }
                }
            }

            public void DescribeTo(SpecInfo spec, MessageFormatter formatter)
            {
                foreach (var givenEvent in _history)
                    givenEvent.DescribeTo(spec, formatter);
            }

            public class GivenEvent
            {
                private readonly object _id;
                private readonly object _event;
                private readonly string _enforcedDescription;

                public GivenEvent(object id, object @event, string enforcedDescription)
                {
                    _id = id;
                    _event = @event;
                    _enforcedDescription = enforcedDescription;
                }

                public void DescribeTo(SpecInfo spec, MessageFormatter formatter)
                {
                    spec.ReportGivenStep(new StepInfo(_enforcedDescription ?? formatter.FormatMessage(_event)));
                }

                public void StoreTo(EventStore eventStore, Func<StepInfo, SpecInfo> reportGivenStep, MessageFormatter formatter)
                {
                    var stepInfo = new StepInfo(_enforcedDescription ?? formatter.FormatMessage(_event));
                    reportGivenStep(stepInfo);
                    eventStore.StoreEvents(_id, new[] { _event }, -1);
                    stepInfo.Pass();
                }
            }

            public void StoreTo(EventStore eventStore, Func<StepInfo, SpecInfo> reportGivenStep, MessageFormatter formatter)
            {
                foreach (var givenEvent in _history)
                {
                    givenEvent.StoreTo(eventStore, reportGivenStep, formatter);
                }
            }
        }

        public class ExpectingWhen
        {
            private readonly ApplicationServiceSpecification _instance;

            public ExpectingWhen(ApplicationServiceSpecification instance)
            {
                _instance = instance;
            }

            public ExpectingThen When(string description, object message)
            {
                _instance._when = new EventEntry(description, message);
                return new ExpectingThen(_instance);
            }

            public ExpectingThen When(object message)
            {
                _instance._when = new EventEntry(null, message);
                return new ExpectingThen(_instance);
            }
        }

        public class ExpectingThen
        {
            private readonly ApplicationServiceSpecification _instance;

            public ExpectingThen(ApplicationServiceSpecification instance)
            {
                _instance = instance;
            }

            public ExpectingAnotherThenOrWireup Then(string description, object @event)
            {
                _instance._expectations.AddExpectation(new EqualityExpectation(@event, description));
                return new ExpectingAnotherThenOrWireup(_instance);
            }

            public ExpectingAnotherThenOrWireup Then(object @event)
            {
                _instance._expectations.AddExpectation(new EqualityExpectation(@event));
                return new ExpectingAnotherThenOrWireup(_instance);
            }

            public ExpectingAnotherThenOrWireup Then<T>(string description, Expression<Func<T, bool>> expectation)
            {
                _instance._expectations.AddExpectation(new Expectation<T>(expectation, description));
                return new ExpectingAnotherThenOrWireup(_instance);
            }

            public ExpectingAnotherThenOrWireup Then<T>(Expression<Func<T, bool>> expectation)
            {
                _instance._expectations.AddExpectation(new Expectation<T>(expectation));
                return new ExpectingAnotherThenOrWireup(_instance);
            }

            public ExpectingWireup NothingShouldHappen()
            {
                _instance._expectations.AddExpectation(new MessageCount(0));
                return new ExpectingWireup(_instance);
            }
        }

        public class ExpectingAnotherThenOrWireup
        {
            private readonly ApplicationServiceSpecification _instance;

            public ExpectingAnotherThenOrWireup(ApplicationServiceSpecification instance)
            {
                _instance = instance;
            }

            public ExpectingAnotherThenOrWireup And(string description, object @event)
            {
                _instance._expectations.AddExpectation(new EqualityExpectation(@event, description));
                return this;
            }

            public ExpectingAnotherThenOrWireup And(object @event)
            {
                _instance._expectations.AddExpectation(new EqualityExpectation(@event));
                return this;
            }

            public ExpectingAnotherThenOrWireup And<T>(string description, Expression<Func<T, bool>> expectation)
            {
                _instance._expectations.AddExpectation(new Expectation<T>(expectation, description));
                return this;
            }

            public ExpectingAnotherThenOrWireup And<T>(Expression<Func<T, bool>> expectation)
            {
                _instance._expectations.AddExpectation(new Expectation<T>(expectation));
                return this;
            }

            public ApplicationServiceSpecification Wireup(Func<Repository, EventStore, Router> setup)
            {
                return new ExpectingWireup(_instance).WireUp(setup);
            }

            public ExpectingAnotherThenOrWireup AndTotalNumberOfEventsIs(int count)
            {
                _instance._expectations.AddExpectation(new MessageCount(count));
                return this;
            }
        }

        public class ExpectingWireup
        {
            private readonly ApplicationServiceSpecification _instance;

            public ExpectingWireup(ApplicationServiceSpecification instance)
            {
                _instance = instance;
            }

            public ApplicationServiceSpecification WireUp(Func<Repository, EventStore, Router> setup)
            {
                _instance._wireup = setup;
                return _instance;
            }
        }
    }
}