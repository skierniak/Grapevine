using System.Collections.Generic;
using System.Linq;

namespace Grapevine.Server
{
    public delegate void ClusterEventHandler();

    /// <summary>
    /// Container class to store and manage starting and stopping multiple RestServer objects
    /// </summary>
    public class RestCluster
    {
        protected internal readonly Dictionary<string, IRestServer> Servers = new Dictionary<string, IRestServer>();
        protected internal bool Started;

        /// <summary>
        /// Gets the number of servers in the cluster
        /// </summary>
        public int Count => Servers.Count;

        /// <summary>
        /// Raised before the cluster starts all servers in the cluster.
        /// </summary>
        public event ClusterEventHandler BeforeStartingAll;

        /// <summary>
        /// Raised after the cluster starts all servers in the cluster.
        /// </summary>
        public event ClusterEventHandler AfterStartingAll;

        /// <summary>
        /// Raised before the cluster stops all servers in the cluster.
        /// </summary>
        public event ClusterEventHandler BeforeStoppingAll;

        /// <summary>
        /// Raised after the cluster stops all servers in the cluster.
        /// </summary>
        public event ClusterEventHandler AfterStoppingAll;

        /// <summary>
        /// Raised before starting each server in the cluster.
        /// </summary>
        public event ServerEventHandler BeforeStartingEach;

        /// <summary>
        /// Raised after starting each server in the cluster.
        /// </summary>
        public event ServerEventHandler AfterStartingEach;

        /// <summary>
        /// Raised before stopping each server in the cluster.
        /// </summary>
        public event ServerEventHandler BeforeStoppingEach;

        /// <summary>
        /// Raised after stopping each server in the cluster.
        /// </summary>
        public event ServerEventHandler AfterStoppingEach;

        /// <summary>
        /// Convenience indexer; synonym for Add and Get methods
        /// </summary>
        /// <param name="label"></param>
        /// <returns>Get or set the instance of IRestServer assigned to the label</returns>
        public IRestServer this[string label]
        {
            get { return Get(label); }
            set { Add(label, value); }
        }

        /// <summary>
        /// Adds a server to the cluster
        /// </summary>
        /// <param name="server"></param>
        /// <returns>Assigned labled of the server that was addded</returns>
        public string Add(IRestServer server)
        {
            Add(server.ListenerPrefix, server);
            return server.ListenerPrefix;
        }

        /// <summary>
        /// Adds a server to the cluster
        /// </summary>
        public void Add(string label, IRestServer server)
        {
            Servers.Add(label, server);
            if (Started) Start(server);
        }

        /// <summary>
        /// Retrieve an IRestServer from the cluster
        /// </summary>
        public IRestServer Get(string label)
        {
            return Servers.ContainsKey(label) ? Servers[label] : null;
        }

        /// <summary>
        /// Stop and remove a server from the cluster
        /// </summary>
        public bool Remove(string label)
        {
            if (!Servers.ContainsKey(label)) return true;

            var server = Get(label);
            if (Started) Stop(server);

            return Servers.Remove(label);
        }

        /// <summary>
        /// Starts each server in the cluster
        /// </summary>
        public void StartAll()
        {
            if (Started) return;

            BeforeStartingAll?.Invoke();

            foreach (var server in Servers.Values.Where(server => !server.IsListening))
            {
                Start(server);
            }

            AfterStartingAll?.Invoke();
            Started = true;
        }

        /// <summary>
        /// Stops each server in the cluster
        /// </summary>
        public void StopAll()
        {
            if (!Started) return;

            BeforeStoppingAll?.Invoke();

            foreach (var server in Servers.Values.Where(_ => _.IsListening))
            {
                Stop(server);
            }

            AfterStoppingAll?.Invoke();
            Started = false;
        }

        protected internal void Start(IRestServer server)
        {
            BeforeStartingEach?.Invoke(server);
            server.Start();
            AfterStartingEach?.Invoke(server);
        }

        protected internal void Stop(IRestServer server)
        {
            BeforeStoppingEach?.Invoke(server);
            server.Stop();
            AfterStoppingEach?.Invoke(server);
        }
    }
}