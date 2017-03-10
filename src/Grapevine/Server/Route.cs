using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Grapevine.Common;
using Grapevine.Core;

namespace Grapevine.Server
{
    public interface IRoute<in TContext>
    {
        /// <summary>
        /// Gets the generic delegate that will be run when the route is invoked
        /// </summary>
        Action<TContext> Delegate { get; }

        /// <summary>
        /// Gets or sets an optional description for the route
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Gets a value that indicates whether the route is enabled
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets the HttpMethod that this route responds to; defaults to HttpMethod.ALL
        /// </summary>
        HttpMethod HttpMethod { get; }

        /// <summary>
        /// Gets a unique name for function that will be invoked in the route, internally assigned
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Get the PathInfo that this method responds to
        /// </summary>
        string PathInfo { get; }

        /// <summary>
        /// Get the PathInfo regular expression used to match this method to requests
        /// </summary>
        Regex PathInfoPattern { get; }

        /// <summary>
        /// Gets a value indicating whether the route matches the given IHttpContext
        /// </summary>
        /// <param name="context"></param>
        /// <returns>bool</returns>
        bool Matches(TContext context);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        void Invoke(TContext context);
    }

    public class Route : IRoute<IInboundHttpContext>
    {
        /// <summary>
        /// The pattern keys specified in the PathInfo
        /// </summary>
        protected readonly List<string> PatternKeys;

        public Action<IInboundHttpContext> Delegate { get; }

        public string Description { get; set; }

        public bool Enabled { get; set; }

        public HttpMethod HttpMethod { get; }

        public string Name { get; }

        public string PathInfo { get; }

        public Regex PathInfoPattern { get; }

        public Route(Action<IInboundHttpContext> action, HttpMethod httpMethod, string pathInfo)
        {
            Delegate = action;
            HttpMethod = httpMethod;
            PathInfo = pathInfo;

            Enabled = true;
            PathInfoPattern = new Regex("");
        }

        public bool Matches(IInboundHttpContext context)
        {
            throw new NotImplementedException();
        }

        public void Invoke(IInboundHttpContext context)
        {
            throw new NotImplementedException();
        }
    }
}
