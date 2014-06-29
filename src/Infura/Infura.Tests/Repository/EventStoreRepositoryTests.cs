using System;
using System.Collections.Generic;
using Infura.EventSourcing;
using Machine.Specifications;

namespace Infura.Tests.Repository
{
    public class EventStoreRepositorySubjectAttribute : SubjectAttribute
    {
        public EventStoreRepositorySubjectAttribute() : base("EventStoreRepository")
        {
        }
    }

    public class when_loading_aggregates
    {
        static aggregate_loading_helper helper;

        Establish context = () =>
        {
            var eventStore = new InMemoryEventStore();
            var repository = new EventStoreRepository(eventStore);
               
            helper = new aggregate_loading_helper(repository);
        };

        It should_return_saved_aggregate = () =>
            helper.VerifyFindingOfSavedAggregate();

        It should_not_find_unsaved_aggreagate = () =>
            helper.VerifyNotFindingUnsavedAggreagte();
    }
}