using System;

namespace Grapevine.Core.Exceptions
{
    /// <summary>
    /// Thrown when the http host is unable to start.
    /// </summary>
    public class UnableToStartHostException : Exception
    {
        public UnableToStartHostException(string message) : base(message) { }

        public UnableToStartHostException(string message, Exception inner) : base(message, inner) { }

        public UnableToStartHostException(Type type, Exception inner = null) : base($"An error occured when trying to start the {type.FullName}", inner) { }
    }
}
