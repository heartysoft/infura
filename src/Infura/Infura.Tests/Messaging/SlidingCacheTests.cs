using System;
using Infura.EventSourcing;
using Infura.InMemoryPublishing;
using Machine.Specifications;

namespace Infura.Tests.Messaging
{
    public class SlidingCacheSubjectAttribute : SubjectAttribute
    {
        public SlidingCacheSubjectAttribute() : base("Sliding Cache")
        {
        }    
    }

    [SlidingCacheSubject]
    public class when_handling_message_when_messageId_is_not_in_cache
    {
        static int handledCount;

        Establish context = () =>
        {
            var cache = new SlidingCacheDispatcher(2, x => handledCount++);
            cache.Publish(new StoredEvent("1", "1", 1, new DateTime(2010, 1, 1), null));
        };

        It should_handle_message = () => handledCount.ShouldEqual(1);
    }

    [SlidingCacheSubject]
    public class when_handling_message_when_messageId_is_in_cache
    {
        static int handledCount;

        Establish context = () =>
        {
            var cache = new SlidingCacheDispatcher(2, x=> handledCount++);
            cache.Publish(new StoredEvent("1", "1", 1, new DateTime(2010, 1, 1), null));
            cache.Publish(new StoredEvent("1", "1", 1, new DateTime(2010, 1, 1), null));
        };

        It should_not_handle_same_message = () => handledCount.ShouldEqual(1);
    }

    [SlidingCacheSubject]
    public class when_handling_message_with_messageId_that_was_but_is_no_longer_in_cache
    {
        static int handledCount;

        Establish context = () =>
        {
            var cache = new SlidingCacheDispatcher(2, x => handledCount++);
            cache.Publish(new StoredEvent("1", "1", 1, new DateTime(2010, 1, 1), null));
            cache.Publish(new StoredEvent("2", "1", 2, new DateTime(2010, 1, 1), null));
            cache.Publish(new StoredEvent("3", "1", 3, new DateTime(2010, 1, 1), null));
            cache.Publish(new StoredEvent("1", "1", 1, new DateTime(2010, 1, 1), null));
        };

        It should_handle_messsage = () =>
            handledCount.ShouldEqual(4);
    }

    [SlidingCacheSubject]
    public class when_handling_duplicate_of_first_message_with_full_cache
    {
        static int handledCount;

        Establish context = () =>
        {
            var cache = new SlidingCacheDispatcher(2, x => handledCount++);
            cache.Publish(new StoredEvent("1", "1", 1, new DateTime(2010, 1, 1), null));
            cache.Publish(new StoredEvent("2", "1", 2, new DateTime(2010, 1, 1), null));
            cache.Publish(new StoredEvent("1", "1", 1, new DateTime(2010, 1, 1), null));
        };

        It should_not_publish_message = () =>
            handledCount.ShouldEqual(2);
    }
}