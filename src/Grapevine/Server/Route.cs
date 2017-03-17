using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Grapevine.Common;
using Grapevine.Core;
using Grapevine.Exceptions;
using System.Linq;

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
            PathInfoPattern = PatternParser.GenerteRegEx(PathInfo);
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

    public static class MethodInfoExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        internal static Action<T> ConvertToAction<T>(this MethodInfo methodInfo)
        {
            methodInfo.IsRestRouteEligible<T>(true); // will throw an aggregate exception if the method is not eligible

            // Static method
            if (methodInfo.IsStatic || methodInfo.ReflectedType == null)
            {
                return context => { methodInfo.Invoke(null, new object[] {context}); };
            }

            // Generate a new instance every invocation
            return context =>
            {
                var instance = Activator.CreateInstance(methodInfo.ReflectedType);
                try
                {
                    methodInfo.Invoke(instance, new object[] { context });
                }
                finally
                {
                    instance.TryDisposing();
                }
            };
        }

        /// <summary>
        /// Returns a value indicating that the referenced method can be used to create a Route object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="method"></param>
        /// <param name="throwExceptionWhenFalse"></param>
        /// <returns></returns>
        internal static bool IsRestRouteEligible<T>(this MethodInfo method, bool throwExceptionWhenFalse = false)
        {
            if (method == null) throw new ArgumentNullException(nameof(method));

            var exceptions = new List<Exception>();

            // Can the method be invoked?
            if (!method.CanInvoke()) exceptions.Add(new InvalidRouteMethodException($"{method.Name} cannot be invoked"));

            // Does the type have a parameterless constructor?
            if (method.ReflectedType != null && !method.ReflectedType.HasParameterlessConstructor()) exceptions.Add(new InvalidRouteMethodException($"{method.ReflectedType} does not have a parameterless constructor"));

            // Can not have a special name (getters and setters)
            if (method.IsSpecialName) exceptions.Add(new InvalidRouteMethodException($"{method.Name} may be treated in a special way by some compilers (such as property accessors and operator overloading methods)"));

            var args = method.GetParameters();

            // Method must have only one argument
            if (args.Length != 1) exceptions.Add(new InvalidRouteMethodException($"{method.Name} must accept one and only one argument"));

            // First argument to method must be of type T
            if (args.Length > 0 && args[0].ParameterType != typeof(T)) exceptions.Add(new InvalidRouteMethodException($"{method.Name}: first argument must be of type {typeof(T).Name}"));

            // Return boolean value
            if (exceptions.Count == 0) return true;
            if (!throwExceptionWhenFalse) return false;

            // Throw exeception
            throw new InvalidRouteMethodExceptions(exceptions.ToArray());
        }

        /// <summary>
        /// Returns a value indicating that the method can be invoked via reflection
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        internal static bool CanInvoke(this MethodInfo methodInfo)
        {
            /*
            * The first set of checks are on the method itself:
            * - Static methods can always be invoked
            * - Abstract methods can never be invoked
            */
            if (methodInfo.IsStatic) return true;
            if (methodInfo.IsAbstract) return false;

            /*
             * The second set of checks are on the type the method
             * comes from. This uses the ReflectedType property,
             * which will be the same property used by the Route
             * class to invoke the method later on.
             * - ReflectedType can not be null
             * - ReflectedType can not be abstract
             * - ReflectedType must be a class (vs an interface or struct, etc.)
             */
            var type = methodInfo.ReflectedType;
            if (type == null) return false;
            if (!type.IsClass) return false;
            if (type.IsAbstract) return false;

            /*
             * If these checks have all passed, then we can be fairly certain
             * that the method can be invoked later on during routing.
             */
            return true;
        }

        /// <summary>
        /// Returns a value indicating whether the type has a constructor that takes no parameters
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool HasParameterlessConstructor(this Type type)
        {
            return (type.GetConstructor(Type.EmptyTypes) != null);
        }

        /// <summary>
        /// Tries to dispose of an object of unknown type
        /// </summary>
        /// <param name="obj"></param>
        internal static bool TryDisposing(this object obj)
        {
            if (!obj.GetType().Implements<IDisposable>()) return true;
            ((IDisposable)obj).Dispose();
            return true;
        }

        /// <summary>
        /// Returns a value indication whether the generic type implements the type parameter specified
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool Implements<T>(this Type type)
        {
            return type.GetInterfaces().Contains(typeof(T));
        }
    }
}