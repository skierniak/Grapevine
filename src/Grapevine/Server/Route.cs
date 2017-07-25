using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Grapevine.Common;
using Grapevine.Core;

namespace Grapevine.Server
{
    public interface IRoute
    {
        /// <summary>
        /// Gets the generic delegate that will be run when the route is invoked
        /// </summary>
        Action<IHttpContext> Delegate { get; }

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
        bool Matches(IHttpContext context);

        IRoute MatchOn(string header, Regex pattern);

        /// <summary>
        /// Invokes the delegate if enabled with the supplied context
        /// </summary>
        /// <param name="context"></param>
        void Invoke(IHttpContext context);
    }

    public class Route : IRoute
    {
        /// <summary>
        /// The pattern keys specified in the PathInfo
        /// </summary>
        protected internal readonly List<string> PatternKeys;

        protected internal readonly Dictionary<string, Regex> MatchesOn;

        public Action<IHttpContext> Delegate { get; protected internal set; }

        public string Description { get; set; }

        public bool Enabled { get; set; }

        public HttpMethod HttpMethod { get; protected internal set; }

        public string Name { get; protected internal set; }

        public string PathInfo { get; protected internal set; }

        public Regex PathInfoPattern { get; protected internal set; }

        public Route(MethodInfo methodInfo, HttpMethod httpMethod, string pathInfo) : this(httpMethod, pathInfo)
        {
            Delegate = methodInfo.ConvertToAction();
            Name = $"{methodInfo.ReflectedType.FullName}.{methodInfo.Name}";
            Description = $"{HttpMethod} {PathInfo} > {Name}";
        }

        public Route(Action<IHttpContext> action, HttpMethod httpMethod, string pathInfo) : this(httpMethod, pathInfo)
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
            PathInfo = !string.IsNullOrWhiteSpace(pathInfo) ? pathInfo : string.Empty;

            PatternKeys = PatternParser.GeneratePatternKeys(PathInfo);
            PathInfoPattern = PatternParser.GenerateRegEx(PathInfo);
            MatchesOn = new Dictionary<string, Regex>();
        }

        public bool Matches(IHttpContext context)
        {
            if (!HttpMethod.IsEquivalentTo(context.Request.HttpMethod) ||
                !PathInfoPattern.IsMatch(context.Request.PathInfo)) return false;

            var headers = context.Request.Headers;
            foreach (var condition in MatchesOn)
            {
                var value = headers.Get(condition.Key) ?? string.Empty;
                if (condition.Value.IsMatch(value)) continue;
                return false;
            }

            return true;
        }

        public IRoute MatchOn(string header, Regex pattern)
        {
            MatchesOn[header] = pattern;
            return this;
        }

        public void Invoke(IHttpContext context)
        {
            if (!Enabled) return;

            context.Request.PathParameters = PatternParser.ExtractParams(context.Request.PathInfo, PathInfoPattern, PatternKeys);

            Delegate.Invoke(context);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is IRoute)) return false;
            var route = (IRoute)obj;

            if (Name != route.Name) return false;

            if (!HttpMethod.IsEquivalentTo(route.HttpMethod)) return false;

            return PathInfoPattern.IsMatch(route.PathInfo) || route.PathInfoPattern.IsMatch(PathInfo);
        }

        public override string ToString()
        {
            return $"{HttpMethod} {PathInfo} > {Name}";
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }
    }
}