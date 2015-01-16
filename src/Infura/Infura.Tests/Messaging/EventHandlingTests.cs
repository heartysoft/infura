using System.Threading.Tasks;
using Machine.Specifications;

namespace Infura.Tests.Messaging
{
    public class SomeEvent
    {
    }

    public class SomeOtherEvent
    {
    }

    public class SomeDerivedEvent : SomeEvent
    {
    }

    public interface SomeInterface
    {
    }

    public class SomeEventImplementingInterface : SomeInterface
    {
    }

    public class SomeHandler
    {
        public bool HandledSomeEvent { get; private set; }
        public bool HandledSomeOtherEvent { get; private set; }
        public bool HandledSomeInterfaceEvent { get; private set; }
        
        public Task Handle(SomeEvent e)
        {
            return Task.Run(()=>HandledSomeEvent = true);
        }

        public Task Handle(SomeOtherEvent e)
        {
            return Task.Run(()=>HandledSomeOtherEvent = true);
        }

        public Task Handle(SomeInterface e)
        {
            return Task.Run(() => HandledSomeInterfaceEvent = true);
        }
    }

    public class when_dispatching_events
    {
        static Bus _bus;
        static SomeHandler _handler1;
        static SomeHandler _handler2;

        Establish context = () =>
        {
            _bus = new Bus();
            _handler1 = new SomeHandler();
            _handler2 = new SomeHandler();
            _bus.Register<SomeEvent>(_handler1.Handle);
            _bus.Register<SomeOtherEvent>(_handler1.Handle);
            _bus.Register<SomeEvent>(_handler2.Handle);
            _bus.Register<SomeOtherEvent>(_handler2.Handle);
        };

        Because of = () => _bus.Publish(new SomeEvent());

        It should_dispatch_to_all_registered_handlers_for_that_event = () =>
        {
            _handler1.HandledSomeEvent.ShouldBeTrue();
            _handler2.HandledSomeEvent.ShouldBeTrue();
        };

        It should_not_dispatch_to_other_handlers = () =>
        {
            _handler1.HandledSomeOtherEvent.ShouldBeFalse();
            _handler2.HandledSomeOtherEvent.ShouldBeFalse();
        };
    }

    public class when_dispatching_derived_event
    {
        static Bus _bus;
        static SomeHandler _handler;

        Establish context = () =>
        {
            _bus = new Bus();
            _handler = new SomeHandler();
            _bus.Register<SomeEvent>(_handler.Handle);
        };

        Because of = () =>
            _bus.Publish(new SomeDerivedEvent());

        It should_dispatch_to_handler_registered_for_base_event = () =>
            _handler.HandledSomeEvent.ShouldBeTrue();
    }

    public class when_dispatching_event_that_implements_an_interface
    {
        static Bus _bus;
        static SomeHandler _handler;

        Establish context = () =>
        {
            _bus = new Bus();
            _handler = new SomeHandler();
            _bus.Register<SomeInterface>(_handler.Handle);
        };

        Because of = () =>
            _bus.Publish(new SomeEventImplementingInterface());

        It should_dispatch_to_handler_registered_for_interface = () =>
            _handler.HandledSomeInterfaceEvent.ShouldBeTrue();
    }
}