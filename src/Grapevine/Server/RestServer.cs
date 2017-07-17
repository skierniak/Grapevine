using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using Grapevine.Common;
using Grapevine.Core;
using Grapevine.Core.Exceptions;
using Grapevine.Core.Logging;
using HttpListener = Grapevine.Core.HttpListener;

namespace Grapevine.Server
{
    /// <summary>
    /// Delegate for the <see cref="IRestServer.BeforeStarting"/>, <see cref="IRestServer.AfterStarting"/>, <see cref="IRestServer.BeforeStopping"/> and <see cref="IRestServer.AfterStopping"/> events
    /// </summary>
    /// <param name="server"></param>
    public delegate void ServerEventHandler(IRestServer server);

    /// <summary>
    /// Provides a programmatically controlled REST implementation for a single prefix.
    /// </summary>
    public interface IRestServer : IDynamicProperties, IDisposable
    {
        /// <summary>
        /// Raised after the server has finished starting.
        /// </summary>
        event ServerEventHandler AfterStarting;

        /// <summary>
        /// Raised after the server has finished stopping.
        /// </summary>
        event ServerEventHandler AfterStopping;

        /// <summary>
        /// Raised before the server starts.
        /// </summary>
        event ServerEventHandler BeforeStarting;

        /// <summary>
        /// Raised before the server stops.
        /// </summary>
        event ServerEventHandler BeforeStopping;

        /// <summary>
        /// Gets the list of all ContentFolder objects used for serving static content.
        /// </summary>
        IList<IContentFolder> ContentFolders { get; }

        /// <summary>
        /// Gets or sets the host name used to create the HttpListener prefix, defaults to localhost.
        /// <para>&#160;</para>
        /// Use "*" to indicate that the HttpListener accepts requests sent to the port if the requested URI does not match any other prefix. Similarly, to specify that the HttpListener accepts all requests sent to a port, replace the host element with the "+" character.
        /// </summary>
        string Host { get; set; }

        /// <summary>
        /// Gets a value that indicates whether the server has started listening.
        /// </summary>
        bool IsListening { get; }

        /// <summary>
        /// Gets the instance of IHttpListener used by the server.
        /// </summary>
        IHttpListener Listener { get; }

        /// <summary>
        /// Gets the prefix created by combining the Protocol, Host and Port properties into a scheme and authority.
        /// </summary>
        string ListenerPrefix { get; }

        /// <summary>
        /// Gets or sets the port number (as a string) used to create the prefix used by the HttpListener for incoming traffic; defaults to 1234.
        /// </summary>
        string Port { get; set; }

        /// <summary>
        /// Gets or sets the instance of IRouter to be used by this server to route incoming HTTP requests.
        /// </summary>
        IRouter Router { get; set; }

        /// <summary>
        /// Gets or sets a value indicating that the listner should use the https protocol instead of http.
        /// <para>&#160;</para>
        /// Note that if you create an HttpListener using https, you must select a Server Certificate for the listener. See the MSDN documentation on the HttpListener class for more information.<br />
        /// https://msdn.microsoft.com/en-us/library/system.net.httplistener(v=vs.110).aspx
        /// </summary>
        bool UseHttps { get; set; }

        /// <summary>
        /// Starts the server, raising BeforeStart and AfterStart events appropriately.
        /// </summary>
        void Start();

        /// <summary>
        /// Stops the server, raising BeforeStop and AfterStop appropriately.
        /// </summary>
        void Stop();
    }

    public class RestServer : DynamicProperties, IRestServer
    {
        public IHttpListener Listener { get; protected internal set; }

        protected UriBuilder UriBuilder = new UriBuilder("http", "localhost", 1234, "/");
        protected GrapevineLogger Logger = GrapevineLogManager.GetCurrentClassLogger();
        protected readonly ManualResetEvent StopEvent = new ManualResetEvent(false);
        protected readonly Thread Listening;
        protected bool IsStopping;
        protected bool IsStarting;

        protected internal bool TestingMode = true;

        public event ServerEventHandler AfterStarting;
        public event ServerEventHandler AfterStopping;
        public event ServerEventHandler BeforeStarting;
        public event ServerEventHandler BeforeStopping;

        public IList<IContentFolder> ContentFolders { get; protected internal set; } = new List<IContentFolder>();

        public RestServer()
        {
            Listener = new HttpListener(this);
            Listening = new Thread(HandleRequests);
        }

        public RestServer(IHttpListener listener)
        {
            Listener = listener;
            Listening = new Thread(HandleRequests);
        }

        public string Host {
            get { return UriBuilder.Host; }
            set
            {
                if (IsListening) throw new ServerStateException();
                UriBuilder.Host = value == "0.0.0.0" ? "+" : value.ToLower();
            }
        }

        public bool IsListening => (Listener != null && Listener.IsListening);

        public string ListenerPrefix => UriBuilder.ToString();

        public string Port
        {
            get { return UriBuilder.Port.ToString(); }
            set
            {
                if (IsListening) throw new ServerStateException();
                UriBuilder.Port = int.Parse(value);
            }
        }

        public IRouter Router { get; set; } = new Router();

        public bool UseHttps
        {
            get { return UriBuilder.Scheme == UriScheme.Https.ToScheme(); }
            set
            {
                if (IsListening) throw new ServerStateException();
                UriBuilder.Scheme = value ? UriScheme.Https.ToScheme() : UriScheme.Http.ToScheme();
            }
        }

        public void Dispose()
        {
            if (IsListening) Stop();
            Listener?.Close();
        }

        public void Start()
        {
            if (IsListening || IsStarting) return;
            if (IsStopping) throw new UnableToStartHostException("Cannot start server until server has finished stopping");
            IsStarting = true;

            try
            {
                OnBeforeStarting();
                if (Router.RoutingTable.Count == 0) Router.Scan();

                Listener.Prefixes?.Clear();
                Listener.Prefixes?.Add(ListenerPrefix);
                Listener.Start();

                if (!TestingMode) Listening.Start();

                Logger.Trace($"Listening: {ListenerPrefix}");
                if (IsListening) OnAfterStarting();
            }
            catch (Exception e)
            {
                throw new UnableToStartHostException($"An error occured when trying to start the {GetType().FullName}", e);
            }
            finally
            {
                IsStarting = false;
            }
        }

        public void Stop()
        {
            if (!IsListening || IsStopping) return;
            if (IsStarting) throw new UnableToStopHostException("Cannot stop server until server has finished starting");
            IsStopping = true;

            try
            {
                OnBeforeStopping();

                StopEvent.Set();
                if (!TestingMode) Listening.Join();
                Listener.Stop();
                StopEvent.Close();

                if (!IsListening) OnAfterStopping();
            }
            catch(Exception e)
            {
                // Catch the HTTPListenerException port-reuse error and throw a better message
                throw new UnableToStopHostException($"An error occured while trying to stop {GetType().FullName}", e);
            }
            finally
            {
                IsStopping = false;
            }
        }

        protected internal void HandleRequests()
        {
            while (Listener.IsListening)
            {
                var context = Listener.BeginGetContext(ContextReady, null);
                if (0 == WaitHandle.WaitAny(new[] {StopEvent, context.AsyncWaitHandle})) return;
            }
        }

        protected internal void ContextReady(IAsyncResult result)
        {
            try
            {
                var context = Listener.EndGetContext(result);
                ThreadPool.QueueUserWorkItem(Router.Route, context);
            }
            catch (ObjectDisposedException)
            {
                /*
                 * Intentionally not doing anything with this
                 * see: https://stackoverflow.com/a/13352359
                 */
            }
            catch (HttpListenerException hle)
            {
                /* Ignores exceptions thrown by incomplete async methods listening for incoming requests */
                if (IsStopping && hle.NativeErrorCode == 995) return;

                Logger.Debug($"Unexpected HttpListenerException Occured (IsStopping:{IsStopping}, NativeErrorCode:{hle.NativeErrorCode})", hle);
            }
            catch (Exception e)
            {
                Logger.Debug(e.Message, e);
            }
        }

        protected internal void OnBeforeStarting()
        {
            if (BeforeStarting == null) return;
            var exceptions = InvokeServerEventHandlers(BeforeStarting.GetInvocationList().Reverse().Cast<ServerEventHandler>());
            if (exceptions.Count > 0) throw new AggregateException(exceptions);
        }

        protected internal void OnAfterStarting()
        {
            if (AfterStarting == null) return;
            var exceptions = InvokeServerEventHandlers(AfterStarting.GetInvocationList().Reverse().Cast<ServerEventHandler>());
            if (exceptions.Count > 0) throw new AggregateException(exceptions);
        }

        protected internal void OnBeforeStopping()
        {
            if (BeforeStopping == null) return;
            var exceptions = InvokeServerEventHandlers(BeforeStopping.GetInvocationList().Reverse().Cast<ServerEventHandler>());
            if (exceptions.Count > 0) throw new AggregateException(exceptions);
        }

        protected internal void OnAfterStopping()
        {
            if (AfterStopping == null) return;
            var exceptions = InvokeServerEventHandlers(AfterStopping.GetInvocationList().Reverse().Cast<ServerEventHandler>());
            if (exceptions.Count > 0) throw new AggregateException(exceptions);
        }

        protected internal List<Exception> InvokeServerEventHandlers(IEnumerable<ServerEventHandler> actions)
        {
            var exceptions = new List<Exception>();

            foreach (var action in actions)
            {
                try
                {
                    action.Invoke(this);
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }

            return exceptions;
        }

        public static void LogToConsole()
        {
            GrapevineLogManager.LogToConsole();
        }
    }
}
