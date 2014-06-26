using System;
using Infura.EventSourcing;
using Infura.InMemoryPublishing;
using Machine.Specifications;

namespace Infura.Tests.Messaging
{
    public class RetryingDispatcherSubject : SubjectAttribute
    {
        public RetryingDispatcherSubject() : base("Retrying Dispatcher")
        {
        }
    }

    public class when_downstream_handler_succeeds
    {
        static RetryingDispatcher _dispatcher;
        static bool _errorHandlerCalled;
        static FailureSimulator _simulator;

        Establish context = () =>
        {
            _simulator = new FailureSimulator(0);
            _dispatcher = 
                new RetryingDispatcher(_simulator.Handle, 
                    x=> _errorHandlerCalled = true, 1);
        };

        Because of = () => 
            _dispatcher.Dispatch(new StoredEvent(1, 1, 1, DateTime.Now, null));


        It should_succeed = () =>
            _simulator.Succeeded.ShouldBeTrue();

        It should_not_call_error_handler = () =>
            _errorHandlerCalled.ShouldBeFalse();
    }

    public class when_downstream_handler_fails_within_limit
    {
        static RetryingDispatcher _dispatcher;
        static bool _errorHandlerCalled;
        static FailureSimulator _simulator;

        Establish context = () =>
        {
            _simulator = new FailureSimulator(1);
            _dispatcher =
                new RetryingDispatcher(_simulator.Handle,
                    x => _errorHandlerCalled = true, 1);
        };

        Because of = () =>
            _dispatcher.Dispatch(new StoredEvent(1, 1, 1, DateTime.Now, null));

        It should_not_call_error_handler = () =>
            _errorHandlerCalled.ShouldBeFalse();

        It should_succeed = () =>
            _simulator.Succeeded.ShouldBeTrue();

    }

    public class when_downstream_handler_fails_maximum_times
    {
        static RetryingDispatcher _dispatcher;
        static bool _errorHandlerCalled;
        static FailureSimulator _simulator;
        static Exception _exception;

        Establish context = () =>
        {
            _simulator = new FailureSimulator(2);
            _dispatcher =
                new RetryingDispatcher(_simulator.Handle,
                    x => _errorHandlerCalled = true, 1);
        };

        Because of = () =>
            _exception = Catch.Exception(() =>
                _dispatcher.Dispatch(new StoredEvent(1, 1, 1, DateTime.Now, null)));

        It should_call_error_handler = () =>
            _errorHandlerCalled.ShouldBeTrue();

        It should_rethrow_exception = () =>
           _exception.ShouldNotBeNull();
    }

    public class FailureSimulator
    {
        int _failuresRemaining;
        public bool Succeeded;

        public FailureSimulator(int numberOfFailuresBeforeSucceeding)
        {
            _failuresRemaining = numberOfFailuresBeforeSucceeding;
        }

        public void Handle(StoredEvent e)
        {
            if(_failuresRemaining > 0)
            {
                _failuresRemaining--;
                throw new InvalidOperationException();
            }

            Succeeded = true;
        }
    }
}