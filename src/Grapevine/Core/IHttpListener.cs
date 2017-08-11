using System;
using System.Collections.Generic;
using Grapevine.Server;

namespace Grapevine.Core
{
    /// <summary>
    /// Interface wrapper for a programmatically controlled HTTP protocol listener
    /// </summary>
    public interface IHttpListener
    {
        /// <summary>
        /// Holds a references to the underlying abstracted implementation
        /// </summary>
        object Advanced { get; }

        /// <summary>
        /// 
        /// </summary>
        bool IsListening { get; }

        /// <summary>
        /// 
        /// </summary>
        ICollection<string> Prefixes { get; }

        /// <summary>
        /// 
        /// </summary>
        IRestServer Server { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        IAsyncResult BeginGetContext(AsyncCallback callback, object state);

        /// <summary>
        /// 
        /// </summary>
        void Close();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="asyncResult"></param>
        /// <returns></returns>
        IHttpContext EndGetContext(IAsyncResult asyncResult);

        /// <summary>
        /// 
        /// </summary>
        void Start();

        /// <summary>
        /// 
        /// </summary>
        void Stop();
    }

    /// <summary>
    /// Wrapper for an instance of System.Net.HttpListener with selective functionality exposed
    /// </summary>
    public class HttpListener : IHttpListener
    {
        object IHttpListener.Advanced => Advanced;

        public System.Net.HttpListener Advanced { get; }

        public bool IsListening => Advanced.IsListening;

        public ICollection<string> Prefixes => Advanced.Prefixes;

        public IRestServer Server { get; protected internal set; }

        public HttpListener()
        {
            Advanced = new System.Net.HttpListener();
        }

        public HttpListener(IRestServer server)
        {
            Server = server;
            Advanced = new System.Net.HttpListener();
        }

        public IAsyncResult BeginGetContext(AsyncCallback callback, object state)
        {
            return Advanced.BeginGetContext(callback, state);
        }

        public void Close()
        {
            Advanced.Close();
        }

        public IHttpContext EndGetContext(IAsyncResult asyncResult)
        {
            var result = Advanced.EndGetContext(asyncResult);
            var context = new HttpContext(result, Server);
            return context;
        }

        public void Start()
        {
            Advanced.Start();
        }

        public void Stop()
        {
            Advanced.Stop();
        }
    }
}
