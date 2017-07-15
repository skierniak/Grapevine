using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using Grapevine.Common;

namespace Grapevine.Core
{
    public interface IHttpRequest
    {
        /// <summary>
        /// Gets the MIME type of the body data included in the request
        /// </summary
        ContentType ContentType { get; }

        /// <summary>
        /// Gets the collection of header name/value pairs sent in the request
        /// </summary>
        NameValueCollection Headers { get; }

        /// <summary>
        /// Gets the HTTPMethod specified by the client
        /// </summary>
        HttpMethod HttpMethod { get; }

        /// <summary>
        /// A value that represents a unique identifier for this request
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets a representation of the HttpMethod and PathInfo of the request
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the URL information (without the host, port or query string) requested by the client
        /// </summary>
        string PathInfo { get; }

        /// <summary>
        /// Gets or sets a dictionary of parameters provided in the PathInfo as identified by the processing route
        /// </summary>
        Dictionary<string, string> PathParameters { get; set; }

        /// <summary>
        /// Gets the query string included in the request
        /// </summary>
        NameValueCollection QueryString { get; }
    }

    public class HttpRequest : IHttpRequest
    {
        public HttpListenerRequest _request { get; protected internal set; }

        public ContentType ContentType { get; protected internal set; }

        public NameValueCollection Headers { get; protected internal set; }

        public HttpMethod HttpMethod { get; protected internal set; }

        public string Id { get; protected internal set; }

        public string Name { get; protected internal set; }

        public string PathInfo { get; protected internal set; }

        public Dictionary<string, string> PathParameters { get; set; }

        public NameValueCollection QueryString { get; protected internal set; }

        protected internal HttpRequest(HttpListenerRequest request)
        {
            _request = request;

            PathInfo = _request.RawUrl.Split(new[] { '?' }, 2)[0];
            ContentType = ContentTypes.FromString(_request.ContentType);
            HttpMethod = HttpMethods.FromString(_request.HttpMethod);
        }
    }
}
