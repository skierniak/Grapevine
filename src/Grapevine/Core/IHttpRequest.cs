using System.Collections.Specialized;
using System.Net;
using Grapevine.Common;

namespace Grapevine.Core
{
    public interface IHttpRequest
    {
        ContentType ContentType { get; }

        NameValueCollection Headers { get; }

        HttpMethod HttpMethod { get; }

        string PathInfo { get; }

        NameValueCollection QueryString { get; }
    }

    public class HttpRequest : IHttpRequest
    {
        public HttpListenerRequest Advanced { get; protected internal set; }

        public ContentType ContentType { get; protected internal set; }

        public NameValueCollection Headers { get; protected internal set; }

        public HttpMethod HttpMethod { get; protected internal set; }
        public string PathInfo { get; protected internal set; }

        public NameValueCollection QueryString { get; protected internal set; }

        protected internal HttpRequest(HttpListenerRequest request)
        {
            Advanced = request;

            PathInfo = Advanced.RawUrl.Split(new[] { '?' }, 2)[0];
            ContentType = ContentTypes.FromString(Advanced.ContentType);
            HttpMethod = HttpMethods.FromString(Advanced.HttpMethod);
        }
    }
}
