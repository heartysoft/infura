using System;
using dokimi.core;
using dokimi.core.dokimi.core.Specs.ApplicationService;
using dokimi.core.Specs.ApplicationService;
using dokimi.nunit;

namespace Infura.Tests.dokimiTesting
{
    public abstract class ExampleSpecification : NUnitSpecificationTest
    {
    }

    public class ExampleCategory : SpecificationCategory
    {
        public ExampleCategory() : base("example", "account")
        {
        }
    }

    public class account_register : ExampleSpecification
    {
        public Specification should_work()
        {
            var accountId = Guid.NewGuid();
            var spec =
                Specifications.Catalog
                    .ApplicationServiceSpecification<ExampleCategory>()
                    .When(new RegisterAccount(accountId, 100M))
                    .Then(new AccountRegistered(accountId, 100M))
                    .Wireup((repo, _) =>
                    {
                        var handler = new AccountApplicationService(repo);

                        return Router.Create()
                            .Route<RegisterAccount>(handler.Handle);
                    });

            return spec;
        }
    }

    public class account_debit : ExampleSpecification
    {
        public Specification should_work()
        {
            var accountId = Guid.NewGuid();
            var spec =
                Specifications.Catalog
                    .ApplicationServiceSpecification<ExampleCategory>()
                    .ForStream(accountId)
                    .Event(new AccountRegistered(accountId, 100M))
                    .When(new DebitAccount(accountId, 5M))
                    .Then(new AccountDebited(accountId, 5M, 100M, 95M))
                    .Wireup((repo, _) =>
                    {
                        var handler = new AccountApplicationService(repo);

                        return Router.Create()
                            .Route<DebitAccount>(handler.Handle);
                    });

            return spec;
        }
    }

    public class should_allow_multiple_transactions : ExampleSpecification
    {
        public Specification when_debiting()
        {
            var accountId = Guid.NewGuid();

            return 
                Specifications.Catalog
                .ApplicationServiceSpecification<ExampleCategory>()
                .ForStream(accountId)
                .Event("Account X registered with £100", new AccountRegistered(accountId, 100M))
                .Event("Account X debited £10", new AccountDebited(accountId, 10M, 100M, 90M))
                .When("Debit attempt on X for £10", new DebitAccount(accountId, 10M))
                .Then("Account X debited £10", new AccountDebited(accountId, 10M, 90M, 80))
                .Wireup((repo, _) =>
                {
                    var handler = new AccountApplicationService(repo);

                    return Router.Create()
                        .Route<DebitAccount>(handler.Handle);
                });
                
        }
    }

    public class overdraft_attempt_should_fail : ExampleSpecification
    {
        public Specification when_debiting()
        {
            var accountId = Guid.NewGuid();

            return
                Specifications.Catalog
                .ApplicationServiceSpecification<ExampleCategory>()
                .ForStream(accountId)
                .Event("Account X registered with £100", new AccountRegistered(accountId, 100M))
                .Event("Account X debited £10", new AccountDebited(accountId, 100M, 100M, 0M))
                .When("Debit attempt on X for £10", new DebitAccount(accountId, 10M))
                .ExpectException<NotEnoughFundsException>()
                .Wireup((repo, es) =>
                {
                    var handler = new AccountApplicationService(repo);

                    return Router.Create()
                        .Route<DebitAccount>(handler.Handle);
                });
        }
    }
}
