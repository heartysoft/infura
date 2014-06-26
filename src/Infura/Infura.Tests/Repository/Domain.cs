using System;

namespace Infura.Tests.Repository
{
    public class Person : Aggregate
    {
        public string Name { get; private set; }

        public Person(Guid id, string name) : base(id)
        {
            Apply(new PersonRegisteredEvent(id, name));
        }

        private Person()
        {
        }

        public void UpdateName(string newName)
        {
            Apply(new PersonNameUpdatedEvent((Guid)Id, newName));
        }

        void UpdateFrom(PersonRegisteredEvent @event)
        {
            Name = @event.Name;
        }

        void UpdateFrom(PersonNameUpdatedEvent @event)
        {
            Name = @event.NewName;
        }
    }

    public class PersonRegisteredEvent
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }

        public PersonRegisteredEvent(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }

    public class PersonNameUpdatedEvent
    {
        public Guid Id { get; private set; }
        public string NewName { get; private set; }

        public PersonNameUpdatedEvent(Guid id, string newName)
        {
            Id = id;
            NewName = newName;
        }
    }
}