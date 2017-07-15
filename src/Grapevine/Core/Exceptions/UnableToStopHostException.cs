using System;

namespace Grapevine.Core.Exceptions
{
    /// <summary>
    /// Thrown when the http host is unable to stop.
    /// </summary>
    public class UnableToStopHostException : Exception
    {
        public UnableToStopHostException(string message) : base(message) { }

        public UnableToStopHostException(string message, Exception inner) : base(message, inner) { }
    }
}
