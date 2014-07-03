using System;
using System.Collections.Generic;
using System.Threading;
using Infura;
using Infura.EventSourcing;

namespace dokimi.core.Specs.ApplicationService
{
    public partial class ApplicationServiceSpecification : Specification
    {
        private GivenContext _given;
        private EventEntry _when;
        private readonly Expectations _expectations = new Expectations();
        private Func<Repository, EventStore, Router> _wireup;
        private Func<CancellationToken, EventStore> _eventStoreFactory = _ => new InMemoryEventStore();

        public ApplicationServiceSpecification UseEventStoreFacotry(Func<CancellationToken, EventStore> es)
        {
            _eventStoreFactory = es;
            return this;
        }

        public void EnrichDescription(SpecInfo spec, MessageFormatter formatter)
        {
            _given.DescribeTo(spec, formatter);
            _when.DescribeTo(spec.ReportWhenStep, formatter);
            _expectations.DescribeTo(spec, formatter);
        }

        public SpecInfo Run(SpecInfo spec, MessageFormatter formatter)
        {
            //var givenSteps = new StepInfo[_given.Count];

            //for (int i = 0; i < _given.Count; i++)
            //{
            //    var given = _given[i];
            //    int i1 = i;
            //    given.DescribeTo(s =>
            //    {
            //        var stepInfo = new StepInfo(s);
            //        givenSteps[i1] = stepInfo;
            //        return spec.ReportGivenStep(stepInfo);
            //    }, formatter);
            //}

            bool givenDescribed=false, whenDescribed = false;

            var prepareStep = new StepInfo("Setup completed");
            spec.ReportGivenStep(prepareStep);

            var token = new CancellationTokenSource();

            var events = new List<object>();
            

            try
            {
                //wireup
                var eventStore = _eventStoreFactory(token.Token);
                eventStore.AdjustDispatcher(x => events.Add(x.EventData));
                var repo = new EventStoreRepository(eventStore);
                var router = _wireup(repo, eventStore);
                
                _given.StoreTo(eventStore, spec.ReportGivenStep, formatter);
                givenDescribed = true;

                events.Clear();
                prepareStep.Pass();

                //run when
                _when.RunTo(router);
                _when.DescribeTo(spec.ReportWhenStep, formatter);
                spec.When.Pass();
                whenDescribed = true;

// ReSharper disable once SuspiciousTypeConversion.Global
                var store = eventStore as IDisposable;
                if(store != null)
                    store.Dispose();
            }
            catch
            {
                if(!givenDescribed)
                    _given.DescribeTo(spec, formatter);

                if(!whenDescribed)
                    _when.DescribeTo(spec.ReportWhenStep, formatter);

                _expectations.DescribeTo(spec, formatter);
                throw;
            }
            finally
            {
                token.Cancel();
            }

            //then
            _expectations.Verify(events.ToArray(), spec, formatter);

            return spec;
        }

        public SpecificationCategory SpecificationCategory { get; private set; }

        protected ApplicationServiceSpecification(SpecificationCategory category)
        {
            SpecificationCategory = category;
        }

        public static GivenContext New(SpecificationCategory category)
        {
            var instance = new ApplicationServiceSpecification(category);
            instance._given = new GivenContext(instance);
            return instance._given;
        }

        public class EventEntry
        {
            private readonly string _description;
            private readonly object _event;

            public EventEntry(string description, object @event)
            {
                _description = description;
                _event = @event;
            }

            public void DescribeTo(Func<string, SpecInfo> func, MessageFormatter formatter)
            {
                func(_description ?? formatter.FormatMessage(_event));
            }

            public void RunTo(Router router)
            {
                router.Handle(_event);
            }
        }
    }
}