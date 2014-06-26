namespace Infura.Tests.EventStore
{
    public class SomeEvent
    {
        public int Id { get; private set; }
        public string Name { get; private set; }

        public SomeEvent(int id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    public class SomeOtherEvent
    {
        public int Id { get; private set; }
        public string NewName { get; private set; }

        public SomeOtherEvent(int id, string newName)
        {
            Id = id;
            NewName = newName;
        }
    }

}