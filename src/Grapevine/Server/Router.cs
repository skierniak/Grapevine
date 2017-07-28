using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Grapevine.Common;
using Grapevine.Core;
using Grapevine.Core.Exceptions;
using Grapevine.Core.Logging;
using HttpStatusCode = Grapevine.Common.HttpStatusCode;

namespace Grapevine.Server
{
    /// <summary>
    /// Delegate for the <see cref="IRouter.BeforeRouting"/> and <see cref="IRouter.AfterRouting"/> events
    /// </summary>
    /// <param name="context">The <see cref="IHttpContext"/> that is being routed.</param>
    public delegate void RoutingEventHandler(IHttpContext context);

    /// <summary>
    /// Provides a mechanism to register routes and invoke them according to the produced routing table
    /// </summary>
    public interface IRouter
    {
        /// <summary>
        /// Raised after a request has completed invoking matching routes
        /// </summary>
        event RoutingEventHandler AfterRouting;

        /// <summary>
        /// Raised prior to sending any request though matching routes
        /// </summary>
        event RoutingEventHandler BeforeRouting;

        /// <summary>
        /// Scan the assembly for routes based on inclusion and exclusion rules
        /// </summary>
        IRouteScanner Scanner { get; set; }

        /// <summary>
        /// Gets a list of registered routes in the order they were registered
        /// </summary>
        IList<IRoute> RoutingTable { get; }

        /// <summary>
        /// Adds the routes in router parameter to the end of the current routing table
        /// </summary>
        /// <param name="router"></param>
        /// <returns>IRouter</returns>
        IRouter Import(IRouter router);

        /// <summary>
        /// Inserts the routes into the routing table at the specified index
        /// </summary>
        int InsertAt(int index, IRoute route);

        /// <summary>
        /// Adds the route to the routing table
        /// </summary>
        /// <param name="route"></param>
        /// <returns>IRouter</returns>
        IRouter Register(IRoute route);

        /// <summary>
        /// Adds all RestRoutes returned from RouteScanner.Scan() to the routing table
        /// </summary>
        /// <returns>IRouter</returns>
        IRouter Scan();

        /// <summary>
        /// Routes the IHttpContext through all enabled registered routes that match the IHttpConext provided
        /// </summary>
        /// <param name="state"></param>
        void Route(object state);

        /// <summary>
        /// Routes the IHttpContext through the list of routes provided
        /// </summary>
        /// <param name="context"></param>
        /// <param name="routing"></param>
        void Route(IHttpContext context, IList<IRoute> routing);

        /// <summary>
        /// Gets a list of enabled registered routes that match the IHttpContext provided
        /// </summary>
        /// <param name="context"></param>
        /// <returns>IList&lt;IRoute&gt;</returns>
        IList<IRoute> RoutesFor(IHttpContext context);
    }

    public class Router : IRouter
    {
        public static Dictionary<HttpStatusCode, Action<IHttpContext>> GlobalErrorHandlers =
            new Dictionary<HttpStatusCode, Action<IHttpContext>>();

        public Dictionary<HttpStatusCode, Action<IHttpContext>> LocalErrorHandlers =
            new Dictionary<HttpStatusCode, Action<IHttpContext>>();

        public static readonly string ConnectionAbortedMsg = "Connection aborted by client";
        public static readonly string UnknownListenerExceptionMsg = "An error occured while attempting to respond to the request";

        protected internal readonly IList<IRoute> RegisteredRoutes = new List<IRoute>();

        public event RoutingEventHandler AfterRouting;
        public event RoutingEventHandler BeforeRouting;

        public IRouteScanner Scanner { get; set; } = new RouteScanner();
        public IList<IRoute> RoutingTable => RegisteredRoutes.ToList().AsReadOnly();

        protected internal GrapevineLogger Logger { get; set; } = GrapevineLogManager.GetCurrentClassLogger();

        public IRouter Import(IRouter router)
        {
            AppendRoutingTable(router.RoutingTable);
            (router as Router)?.TransferEventHandlers(this);
            return this;
        }

        public int InsertAt(int index, IRoute route)
        {
            if (index < 0 || index > RegisteredRoutes.Count || RegisteredRoutes.Any(r => r.Name == route.Name)) return index;
            RegisteredRoutes.Insert(index, route);
            return index + 1;
        }

        public IRouter Register(IRoute route)
        {
            AppendRoutingTable(route);
            return this;
        }

        public IRouter Scan()
        {
            AppendRoutingTable(Scanner.Scan());
            return this;
        }

        public void Route(object state)
        {
            var context = state as IHttpContext;
            if (context == null) return;

            try
            {
                if (context.Request.HttpMethod == HttpMethod.GET)
                {
                    foreach (var folder in context.Server.ContentFolders)
                    {
                        folder.SendFile(context);
                        if (context.WasRespondedTo) return;
                    }
                }

                if (context.Response.StatusCode == HttpStatusCode.Ok) Route(context, RoutesFor(context));
            }
            catch (Exception e)
            {
                Logger.Error(e.Message, e);
            }

            if (!context.WasRespondedTo) ErrorHandling(context);
        }

        public void Route(IHttpContext context, IList<IRoute> routing)
        {
            if (routing == null || !routing.Any())
            {
                context.Response.StatusCode = HttpStatusCode.NotFound;
                return;
            }

            var total = routing.Count;
            var counter = 0;

            Logger.Trace($"{context.Request.Name} has {total} matching routes", context.Request.Id);

            try
            {
                OnBeforeRouting(context);

                foreach (var route in routing.Where(route => route.Enabled))
                {
                    if (context.WasRespondedTo || context.Response.StatusCode != HttpStatusCode.Ok) break;

                    counter++;
                    route.Invoke(context);

                    Logger.Trace($"{counter}/{total} {route.Name}", context.Request.Id);
                }
            }
            catch (HttpListenerException e)
            {
                var msg = e.NativeErrorCode == 64
                    ? ConnectionAbortedMsg
                    : UnknownListenerExceptionMsg;
                Logger.Warn(msg, e, context.Request.Id);
            }
            finally
            {
                OnAfterRouting(context);
                Logger.Trace($"{counter} of {total} routes invoked", context.Request.Id);
            }
        }

        public IList<IRoute> RoutesFor(IHttpContext context)
        {
            return RegisteredRoutes.Where(r => r.Matches(context) && r.Enabled).ToList();
        }

        protected internal void TransferEventHandlers(Router router)
        {
            router.BeforeRouting += BeforeRouting;
            router.AfterRouting += AfterRouting;
        }

        protected internal void ErrorHandling(IHttpContext context)
        {
            if (context.Response.StatusCode == HttpStatusCode.Ok)
                context.Response.StatusCode = HttpStatusCode.InternalServerError;

            var action = LocalErrorHandlers.ContainsKey(context.Response.StatusCode)
                ? LocalErrorHandlers[context.Response.StatusCode]
                : GlobalErrorHandlers.ContainsKey(context.Response.StatusCode)
                    ? GlobalErrorHandlers[context.Response.StatusCode]
                    : null;

            action?.Invoke(context);

            if (action == null || !context.WasRespondedTo)
            {
                context.Response.SendResponse(context.Response.StatusCode);
            }
        }

        /// <summary>
        /// Event handler for when the <see cref="BeforeRouting"/> event is raised
        /// </summary>
        /// <param name="context">The <see cref="IHttpContext"/> being routed</param>
        protected internal void OnBeforeRouting(IHttpContext context)
        {
            BeforeRouting?.Invoke(context);
        }

        /// <summary>
        /// Event handler for when the <see cref="AfterRouting"/> event is raised
        /// </summary>
        /// <param name="context">The <see cref="IHttpContext"/> being routed</param>
        protected internal void OnAfterRouting(IHttpContext context)
        {
            if (AfterRouting == null) return;
            foreach (var action in AfterRouting.GetInvocationList().Reverse().Cast<RoutingEventHandler>())
            {
                action(context);
            }
        }

        /// <summary>
        /// Adds the route to the routing table excluding duplicates
        /// </summary>
        /// <param name="route"></param>
        protected internal void AppendRoutingTable(IRoute route)
        {
            if (RegisteredRoutes.All(r => !route.Equals(r))) RegisteredRoutes.Add(route);
        }

        /// <summary>
        /// Adds the routes to the routing table excluding duplicates
        /// </summary>
        /// <param name="routes"></param>
        protected internal void AppendRoutingTable(IEnumerable<IRoute> routes)
        {
            routes.ToList().ForEach(AppendRoutingTable);
        }
    }

    public static class RouterInterfaceExtensions
    {
        /* Create extensions for:
         * Import
         * Insert
         * Register
         */
    }
}