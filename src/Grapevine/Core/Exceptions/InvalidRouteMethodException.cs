using System;

namespace Grapevine.Core.Exceptions
{
    /// <summary>
    /// Thrown when a method is not eligible to be used as a route
    /// </summary>
    public class InvalidRouteMethodException : Exception
    {
        public InvalidRouteMethodException(string message, params object[] values) : base(string.Format(message, values)) { }
    }

    /// <summary>
    /// An aggregate of exceptions thrown when a method is not eligible to be used as a route
    /// </summary>
    public class InvalidRouteMethodExceptions : AggregateException
    {
        public InvalidRouteMethodExceptions(Exception[] exceptions) : base(exceptions) { }
    }
}