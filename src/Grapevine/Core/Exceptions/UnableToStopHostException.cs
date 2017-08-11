using System;
using Grapevine.Properties;

namespace Grapevine.Core.Exceptions
{
    /// <summary>
    /// Thrown when the http host is unable to stop.
    /// </summary>
    public class UnableToStopHostException : Exception
    {
        public UnableToStopHostException(string message, params object[] values) : base(string.Format(message, values)) { }

        public UnableToStopHostException(Exception inner, string message, params object[] values) : base(string.Format(message, values), inner) { }

        public UnableToStopHostException(Type type, Exception inner = null) : base(string.Format(Messages.UnableToStopRestServerOfType, type.FullName), inner) { }

    }
}
