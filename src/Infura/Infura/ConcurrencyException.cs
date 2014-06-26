using System;

namespace Infura
{
    public sealed class ConcurrencyException : Exception
    {
        private ConcurrencyException(string message, Exception inner):base(message, inner)
        {
        }

        private ConcurrencyException(string message):base(message)
        {
        }


        public ConcurrencyException(string id, long currentVersion, long attemptedVersion, Exception inner)
            : this(getMessage(id, currentVersion, attemptedVersion), inner)
        {
        }

        public ConcurrencyException(string streamId, long currentVersion, long attemptedVersion)
            : this(getMessage(streamId, currentVersion, attemptedVersion))
        {
        }

        private static string getMessage(string streamId, long currentVersion, long attemptedVersion)
        {
            return string.Format(
                "The version of stream {0} in the Event Store is {1} and the attempted version was {2}.",
                streamId,
                currentVersion,
                attemptedVersion);
        }
    }
}