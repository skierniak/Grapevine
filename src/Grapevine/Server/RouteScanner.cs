using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Grapevine.Common;
using Grapevine.Core.Exceptions;
using Grapevine.Core.Logging;

namespace Grapevine.Server
{
    /// <summary>
    /// Scans assemblies in the AppDomain and generates a list of routes for attributed methods in attributed classes.
    /// </summary>
    public interface IRouteScanner
    {
        /// <summary>
        /// Set a rule to exclude the specified assembly when auto-scanning for routes.
        /// </summary>
        /// <param name="assembly"></param>
        void Exclude(Assembly assembly);

        /// <summary>
        /// Set a rule to exclude the specified type when auto-scanning for routes.
        /// </summary>
        /// <param name="type"></param>
        void Exclude(Type type);

        /// <summary>
        /// Set a rule to include the specified assembly when auto-scanning for routes.
        /// </summary>
        /// <param name="assembly"></param>
        void Include(Assembly assembly);

        /// <summary>
        /// Set a rule to include the specified type when auto-scanning for routes.
        /// </summary>
        /// <param name="type"></param>
        void Include(Type type);

        /// <summary>
        /// Generates a list of routes for all RestResource classes found in all assemblies in the current AppDomain.
        /// </summary>
        /// <returns></returns>
        IList<IRoute> Scan(string basePath);

        /// <summary>
        /// Generates a list of routes for all RestResource classes found in the specified assembly.
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="basePath"></param>
        /// <returns></returns>
        IList<IRoute> ScanAssembly(Assembly assembly, string basePath);

        /// <summary>
        /// Generates a list of routes for all RestRoute attributed methods found in the specified class.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="basePath"></param>
        /// <returns></returns>
        IList<IRoute> ScanType(Type type, string basePath);

        /// <summary>
        /// Generates a list of routes for the RestRoute attributed MethodInfo provided and the basePath applied to the PathInfo.
        /// </summary>
        /// <param name="methodInfo"></param>
        /// <param name="basePath"></param>
        /// <returns></returns>
        IList<IRoute> ScanMethod(MethodInfo methodInfo, string basePath);

        /// <summary>
        /// Provides the router with the concrete class to use when generating new routes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        void ImplementRoutesAs<T>() where T : IRoute;
    }

    public class RouteScanner : IRouteScanner
    {
        protected internal readonly IList<Type> ExcludedTypes = new List<Type>();
        protected internal readonly IList<Type> IncludedTypes = new List<Type>();

        protected internal readonly IList<Assembly> ExcludedAssemblies = new List<Assembly>();
        protected internal readonly IList<Assembly> IncludedAssemblies = new List<Assembly>();

        public static readonly List<Assembly> Assemblies;

        private readonly GrapevineLogger _logger;
        protected internal Type RouteImplementation = typeof(Route);

        static RouteScanner()
        {
            Assemblies = new List<Assembly>();
            foreach (
                var assembly in
                    AppDomain.CurrentDomain.GetAssemblies()
                        .Where(a => !a.GlobalAssemblyCache && a.GetName().Name != "Grapevine" && !a.GetName().Name.StartsWith("vshost"))
                        .OrderBy(a => a.FullName))
            { Assemblies.Add(assembly); }
        }

        public RouteScanner()
        {
            _logger = GrapevineLogManager.GetCurrentClassLogger();
        }

        public void ImplementRoutesAs<T>() where T : IRoute
        {
            var type = typeof(T);

            var ctors = type.GetConstructors();


            if (!type.GetConstructors().Any(c =>
            {
                var @params = c.GetParameters();

                if (@params.Length != 3) return false;
                if (@params[0].ParameterType != typeof(MethodInfo)) return false;
                if (@params[1].ParameterType != typeof(HttpMethod)) return false;
                if (@params[2].ParameterType != typeof(string)) return false;

                return true;
            })) throw new MissingConstructorException(type, typeof(MethodInfo), typeof(HttpMethod), typeof(string));

            RouteImplementation = type;
        }

        public void Exclude(Assembly assembly)
        {
            if (!ExcludedAssemblies.Contains(assembly)) ExcludedAssemblies.Add(assembly);
        }

        public void Exclude(Type type)
        {
            if (!ExcludedTypes.Contains(type)) ExcludedTypes.Add(type);
        }

        public void Include(Assembly assembly)
        {
            if (!IncludedAssemblies.Contains(assembly)) IncludedAssemblies.Add(assembly);
        }

        public void Include(Type type)
        {
            if (!IncludedTypes.Contains(type)) IncludedTypes.Add(type);
        }

        public IList<IRoute> Scan(string basePath)
        {
            var routes = new List<IRoute>();
            _logger.Trace($"Scanning {Assemblies.Count} assemblies for routes.");

            foreach (var assembly in Assemblies)
            {
                if (ExcludedAssemblies.Contains(assembly)) continue;
                if (IncludedAssemblies.Count > 0 && !IncludedAssemblies.Contains(assembly)) continue;
                routes.AddRange(ScanAssembly(assembly, basePath));
            }

            _logger.Trace($"Found {routes.Count} routes in all assemblies");
            return routes;
        }

        public IList<IRoute> ScanAssembly(Assembly assembly, string basePath)
        {
            var routes = new List<IRoute>();
            _logger.Trace($"Scanning assembly {assembly.GetName().Name} for routes.");

            foreach (var type in assembly.GetTypes().Where(t => t.IsRestResource()).OrderBy(t => t.Name))
            {
                if (ExcludedTypes.Contains(type)) continue;
                if (IncludedTypes.Count > 0 && !IncludedTypes.Contains(type)) continue;
                routes.AddRange(ScanType(type, basePath));
            }

            _logger.Trace($"Found {routes.Count} routes in assembly {assembly.GetName().Name}.");
            return routes;
        }

        public IList<IRoute> ScanType(Type type, string basePath)
        {
            var routes = new List<IRoute>();
            if (type.IsAbstract || !type.IsClass) return routes;
            _logger.Trace($"Scanning class {type.Name} for routes.");

            var basepath = PathInfoService.GenerateBasePath(basePath, type);
            foreach (var methodInfo in type.GetMethods().Where(m => m.IsRestRoute()).OrderBy(m => m.Name))
            {
                routes.AddRange(ScanMethod(methodInfo, basepath));
            }

            _logger.Trace($"Found {routes.Count} routes in class {type.Name}.");
            return routes;
        }

        public IList<IRoute> ScanMethod(MethodInfo methodInfo, string basePath)
        {
            var routes = new List<IRoute>();
            _logger.Trace($"Scanning method {methodInfo.Name} for routes.");

            var basepath = PathInfoService.SanitizeBasePath(basePath);
            foreach (var attribute in methodInfo.GetCustomAttributes(true).Where(a => true).Cast<RestRoute>())
            {
                var pathinfo = PathInfoService.GeneratePathInfo(attribute.PathInfo, basepath);
                var route = (IRoute) Activator.CreateInstance(RouteImplementation, methodInfo, attribute.HttpMethod, pathinfo);
                _logger.Trace($"Generated route {route.Name}");
                routes.Add(route);
            }

            _logger.Trace($"Found {routes.Count} routes in method {methodInfo.Name}.");
            return routes;
        }
    }

    public static class RouteScannerInterfaceExtensions
    {
        /// <summary>
        /// Set a rule to include the generic type when auto-scanning for routes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scanner"></param>
        public static void Include<T>(this IRouteScanner scanner) where T : class
        {
            scanner.Include(typeof(T));
        }

        /// <summary>
        /// Set a rule to exclude the generic type when auto-scanning for routes.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="scanner"></param>
        public static void Exclude<T>(this IRouteScanner scanner) where T : class
        {
            scanner.Exclude(typeof(T));
        }

        /// <summary>
        /// Generates a list of routes for all RestResource classes found in all assemblies in the current AppDomain.
        /// </summary>
        /// <returns></returns>
        public static IList<IRoute> Scan(this IRouteScanner scanner)
        {
            return scanner.Scan(string.Empty);
        }

        /// <summary>
        /// Generates a list of routes for all RestResource classes found in the specified assembly.
        /// </summary>
        /// <param name="scanner"></param>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static IList<IRoute> ScanAssembly(this IRouteScanner scanner, Assembly assembly)
        {
            return scanner.ScanAssembly(assembly, string.Empty);
        }

        /// <summary>
        /// Generates a list of routes for all RestRoute attributed methods found in the specified class.
        /// </summary>
        /// <param name="scanner"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IList<IRoute> ScanType(this IRouteScanner scanner, Type type)
        {
            return scanner.ScanType(type, string.Empty);
        }

        /// <summary>
        /// Generates a list of routes for the RestRoute attributed MethodInfo provided.
        /// </summary>
        /// <param name="scanner"></param>
        /// <param name="methodInfo"></param>
        /// <returns></returns>
        public static IList<IRoute> ScanMethod(this IRouteScanner scanner, MethodInfo methodInfo)
        {
            return scanner.ScanMethod(methodInfo, string.Empty);
        }
    }
}
