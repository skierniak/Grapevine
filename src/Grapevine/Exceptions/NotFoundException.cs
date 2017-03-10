using System;
using Grapevine.Common;
using Grapevine.Interfaces;

namespace Grapevine.Exceptions
{
    /// <summary>
    /// 
    /// </summary>
    public abstract class NotFoundException : Exception
    {
        protected NotFoundException(string message) : base(message)
        {
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class FileNotFoundException : NotFoundException
    {
        public FileNotFoundException(string pathInfo) : base($"File {pathInfo} was not found")
        {
        }
    }

    /// <summary>
    /// Thrown when no routes are found for the provided context.
    /// </summary>
    public class RouteNotFoundException : NotFoundException
    {
        public RouteNotFoundException(HttpMethod httpMethod, string pathInfo)
            : base($"Route Not Found For {httpMethod} {pathInfo}")
        {
        }
    }
}
