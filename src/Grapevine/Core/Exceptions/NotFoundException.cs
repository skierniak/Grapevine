using System;
using Grapevine.Common;
using Grapevine.Properties;

namespace Grapevine.Core.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class NotFoundException : Exception
    {
        protected NotFoundException(string message) : base(message) { }
    }

    /// <summary>
    /// 
    /// </summary>
    public class FileNotFoundException : NotFoundException
    {
        public FileNotFoundException(string pathInfo) : base(string.Format(Messages.FileNotFound, pathInfo)) { }
    }

    /// <summary>
    /// Thrown when no routes are found for the provided context.
    /// </summary>
    public class RouteNotFoundException : NotFoundException
    {
        public RouteNotFoundException(HttpMethod httpMethod, string pathInfo) : base(string.Format(Messages.RouteNotFound, httpMethod, pathInfo)) { }
    }
}
