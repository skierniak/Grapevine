using System;
using Grapevine.Properties;

namespace Grapevine.Core.Exceptions
{
    /// <summary>
    /// Thrown when the http host is unable to start.
    /// </summary>
    public class UnableToStartHostException : Exception
    {
        public UnableToStartHostException(string message, params object[] values) : base(string.Format(message, values)) { }

        public UnableToStartHostException(Exception inner, string message, params object[] values) : base(string.Format(message, values), inner) { }

        public UnableToStartHostException(Type type, Exception inner = null) : base(string.Format(Messages.UnableToStartRestServerOfType, type.FullName), inner) { }
    }
}
