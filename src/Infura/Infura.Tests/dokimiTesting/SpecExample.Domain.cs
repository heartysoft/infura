using System;
using dokimi.core;
using dokimi.core.dokimi.core.Specs.ApplicationService;
using dokimi.core.Specs.ApplicationService;
using dokimi.nunit;
using Infura.Tests.Messaging;

namespace Infura.Tests.dokimiTesting
{
    public class AccountRegistered
    {
        public Guid AccountId { get; private set; }
        public decimal IntitialBalance { get; private set; }

        public AccountRegistered(Guid accountId, decimal intitialBalance)
        {
            AccountId = accountId;
            IntitialBalance = intitialBalance;
        }
    }

    public class AccountDebited
    {
        public Guid AccountId { get; private set; }
        public decimal Amount { get; private set; }
        public decimal OldBalance { get; private set; }
        public decimal NewBalance { get; private set; }

        public AccountDebited(Guid accountId, decimal amount, decimal oldBalance, decimal newBalance)
        {
            AccountId = accountId;
            Amount = amount;
            OldBalance = oldBalance;
            NewBalance = newBalance;
        }
    }

    public class OverdraftAttempted
    {
        public Guid AccountId { get; private set; }

        public OverdraftAttempted(Guid accountId)
        {
            AccountId = accountId;
        }
    }

    public class AccountLocked
    {
        public Guid AccountId { get; private set; }

        public AccountLocked(Guid accountId)
        {
            AccountId = accountId;
        }
    }

    public class RegisterAccount
    {
        public Guid AccountId { get; private set; }
        public decimal IntitialBalance { get; private set; }

        public RegisterAccount(Guid accountId, decimal intitialBalance)
        {
            AccountId = accountId;
            IntitialBalance = intitialBalance;
        }
    }

    public class DebitAccount
    {
        public Guid AccountId { get; private set; }
        public decimal Amount { get; private set; }

        public DebitAccount(Guid accountId, decimal amount)
        {
            AccountId = accountId;
            Amount = amount;
        }
    }

    public class Account : Aggregate
    {
        private decimal _balance;

        protected Account() { }

        public Account(Guid accountId, decimal initialBalance)
            : base(accountId)
        {
            Apply(new AccountRegistered(accountId, initialBalance));
        }

        public void Debit(decimal amount)
        {
            Apply(new AccountDebited((Guid)Id, amount, _balance, _balance- amount));
        }

        private void UpdateFrom(AccountRegistered e)
        {
            _balance = e.IntitialBalance;
        }

        private void UpdateFrom(AccountDebited e)
        {
            _balance = e.NewBalance;
        }
    }

    public class AccountApplicationService
    {
        private readonly Infura.Repository _repo;

        public AccountApplicationService(Infura.Repository repo)
        {
            _repo = repo;
        }

        public void Handle(RegisterAccount cmd)
        {
            var account = new Account(cmd.AccountId, cmd.IntitialBalance);
            _repo.Save(account);
        }

        public void Handle(DebitAccount cmd)
        {
            var account = _repo.GetById<Account>(cmd.AccountId);
            account.Debit(cmd.Amount);
            _repo.Save(account);
        }
    }
}
