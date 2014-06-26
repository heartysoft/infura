using Machine.Specifications;

namespace Infura.Tests.Messaging
{
    public class CommandHandlingSubjectAttribute : SubjectAttribute
    {
        public CommandHandlingSubjectAttribute()
            : base("Command Handling")
        {
        }
    }

    public class SomeCommand { }
    public class SomeCommandExecutor
    {
        public bool Handled;

        public void Handle(SomeCommand command)
        {
            Handled = true;
        }
    }

    [CommandHandlingSubject]
    public class when_handling_command_with_registered_handler
    {
        static Bus _bus;
        static SomeCommandExecutor _handler;

        Establish context = () =>
        {
            _bus = new Bus();
            _handler = new SomeCommandExecutor();
            _bus.Register<SomeCommand>(_handler.Handle);
        };

        Because of = () => _bus.ExecuteCommand(new SomeCommand());

        It should_dispath_command_to_registered_handler
            = () => _handler.Handled.ShouldBeTrue();
    }
}