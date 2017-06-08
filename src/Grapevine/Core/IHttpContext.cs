using System.Net;
using Grapevine.Server;

namespace Grapevine.Core
{
    public interface IHttpContext
    {
        IHttpRequest Request { get; }

        IHttpResponse Response { get; }

        IRestServer Server { get; }

        bool WasRespondedTo { get; }
    }

    public class HttpContext : IHttpContext
    {
        public HttpListenerContext Advanced { get; protected internal set; }

        public IHttpRequest Request { get; protected internal set; }

        public IHttpResponse Response { get; protected internal set; }

        public IRestServer Server { get; protected internal set; }

        public bool WasRespondedTo => Response.ResponseSent;

        protected internal HttpContext(HttpListenerContext httpListenerContext)
        {
            Advanced = httpListenerContext;
        }
    }
}
