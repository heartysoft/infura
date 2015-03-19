using System;
using Machine.Specifications;

namespace Infura.Tests.Repository
{
    public class aggregate_loading_helper
    {
        readonly Person person;
        readonly Guid id;
        readonly Exception exception;

        public aggregate_loading_helper(Infura.Repository repo)
        {
            id = Guid.NewGuid();
            var p = new Person(id, "John");
            p.UpdateName("Jake");

            repo.Save(p).Wait();

            person = repo.GetById<Person>(id).Result;

            try
            {
                repo.GetById<Person>(20).Wait();
            }
            catch(Exception e)
            {
                exception = e;
            }
        }

        public void VerifyFindingOfSavedAggregate()
        {
            person.Id.ShouldEqual(id);
            person.Name.ShouldEqual("Jake");
        }

        public void VerifyNotFindingUnsavedAggreagte()
        {
			exception.InnerException.ShouldBeOfExactType<AggregateNotFoundException>();
        }
    }
}