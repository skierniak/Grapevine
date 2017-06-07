using System;
using System.Collections.Generic;
using System.Reflection;
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
        /// Gets or sets an optional description for the route that can be useful when logging or debugging
        /// </summary>
        string Description { get; set; }

        /// <summary>
        /// Get or set a value that indicates whether the route should be invoked
        /// </summary>
        bool Enabled { get; set; }

        /// <summary>
        /// Gets the HttpMethod that this route responds to; defaults to HttpMethod.ALL
        /// </summary>
        HttpMethod HttpMethod { get; }

        /// <summary>
        /// Gets an internally assigned unique name for the delegate that will be invoked in the route
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Get the PathInfo pattern that this method responds to
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
        /// Invokes the delegate if enabled with the supplied context
        /// </summary>
        /// <param name="context"></param>
        void Invoke(TContext context);
    }

    public class Route : IRoute<HttpContext>
    {
        /// <summary>
        /// The pattern keys specified in the PathInfo
        /// </summary>
        protected readonly List<string> PatternKeys;

        public Action<HttpContext> Delegate { get; }

        public string Description { get; set; }

        public bool Enabled { get; set; }

        public HttpMethod HttpMethod { get; }

        public string Name { get; }

        public string PathInfo { get; }

        public Regex PathInfoPattern { get; }

        public Route(MethodInfo methodInfo, HttpMethod httpMethod, string pathInfo):this(httpMethod, pathInfo)
        {
            Delegate = methodInfo.ConvertToAction<HttpContext>();
            Name = $"{methodInfo.ReflectedType.FullName}.{methodInfo.Name}";
            Description = $"{HttpMethod} {PathInfo} > {Name}";
        }

        public Route(Action<HttpContext> action, HttpMethod httpMethod, string pathInfo):this(httpMethod, pathInfo)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            Delegate = action;
            Name = $"{Delegate.Method.ReflectedType}.{Delegate.Method.Name}";
            Description = $"{HttpMethod} {PathInfo} > {Name}";
        }

        private Route(HttpMethod httpMethod, string pathInfo)
        {
            Enabled = true;

            HttpMethod = httpMethod;
            PathInfo = (!string.IsNullOrWhiteSpace(pathInfo)) ? pathInfo : string.Empty;

            PatternKeys = PatternParser.GeneratePatternKeys(PathInfo);
            PathInfoPattern = PatternParser.GenerateRegEx(PathInfo);
        }

        public bool Matches(HttpContext context)
        {
            return HttpMethod.IsEquivalentTo(context.Request.HttpMethod) &&
                   PathInfoPattern.IsMatch(context.Request.PathInfo);
        }

        public void Invoke(HttpContext context)
        {
            if (!Enabled) return;

            // get path parameters

            Delegate.Invoke(context);
        }
    }
}