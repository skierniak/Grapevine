using System;

namespace Grapevine.Core.Exceptions
{
    /// <summary>
    /// Thrown when the http host is unable to stop.
    /// </summary>
    public class UnableToStopHostException : Exception
    {
        public UnableToStopHostException(string message, params object[] values) : base(string.Format(message, values)) { }

        public UnableToStopHostException(Exception inner, string message, params object[] values) : base(string.Format(message, values), inner) { }
    }
}
