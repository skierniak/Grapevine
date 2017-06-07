using System.Collections.Specialized;
using System.Net;
using Grapevine.Common;

namespace Grapevine.Core
{
    public interface IHttpRequest<out TRequest>
    {
        TRequest Advanced { get; }

        ContentType ContentType { get; }

        NameValueCollection Headers { get; }

        HttpMethod HttpMethod { get; }

        string PathInfo { get; }

        NameValueCollection QueryString { get; }
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

    public class InboundHttpRequest : IInboundHttpRequest<HttpListenerRequest>
    {
        public HttpListenerRequest Advanced { get; }

        public InboundHttpRequest(System.Net.HttpListenerRequest request)
        {
            Advanced = request;

            PathInfo = Advanced.RawUrl.Split(new[] { '?' }, 2)[0];
            ContentType = ContentTypes.FromString(Advanced.ContentType);
            HttpMethod = HttpMethods.FromString(Advanced.HttpMethod);
        }

        public ContentType ContentType { get; }

        public NameValueCollection Headers => Advanced.Headers;

        public HttpMethod HttpMethod { get; }

        public string PathInfo { get; }

        public NameValueCollection QueryString => Advanced.QueryString;
    }
}
