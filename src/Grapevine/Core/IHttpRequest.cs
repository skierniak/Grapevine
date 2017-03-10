using System.Collections.Specialized;

namespace Grapevine.Core
{
    public interface IHttpRequest<out TRequest>
    {
        TRequest Advanced { get; }

        NameValueCollection Headers { get; }

        string PathInfo { get; }
    }

    public interface IInboundHttpRequest
    {
    }

    public interface IInboundHttpRequest<out TRequest> : IInboundHttpRequest, IHttpRequest<TRequest>
    {
    }

    public interface IOutboundHttpRequest
    {
    }

    public interface IOutboundHttpRequest<out TRequest> : IOutboundHttpRequest, IHttpRequest<TRequest>
    {
    }

    public class InboundHttpRequest : IInboundHttpRequest<System.Net.HttpListenerRequest>
    {
        public System.Net.HttpListenerRequest Advanced { get; }

        public InboundHttpRequest(System.Net.HttpListenerRequest request)
        {
            Advanced = request;

        }

        public NameValueCollection Headers => Advanced.Headers;

        public string PathInfo { get; }
    }
}
