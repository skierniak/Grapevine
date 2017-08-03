using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using Grapevine.Common;

namespace Grapevine.Core
{
    public interface IHttpRequest
    {
        /// <summary>
        /// Gets the MIME type of the body data included in the advanced
        /// </summary>
        ContentType ContentType { get; }

        /// <summary>
        /// Gets the collection of header name/value pairs sent in the advanced
        /// </summary>
        NameValueCollection Headers { get; }

        /// <summary>
        /// Gets the HTTPMethod specified by the client
        /// </summary>
        HttpMethod HttpMethod { get; }

        /// <summary>
        /// A value that represents a unique identifier for this advanced
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Gets a representation of the HttpMethod and PathInfo of the advanced
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
        /// Gets the query string included in the advanced
        /// </summary>
        NameValueCollection QueryString { get; }
    }

    public class HttpRequest : IHttpRequest
    {
        public HttpListenerRequest Advanced { get; }

        public ContentType ContentType { get; }

        public NameValueCollection Headers => Advanced.Headers;

        public HttpMethod HttpMethod { get; }

        public string Id { get; }

        public string Name { get; }

        public string PathInfo { get; }

        public Dictionary<string, string> PathParameters { get; set; }

        public NameValueCollection QueryString => Advanced.QueryString;

        protected internal HttpRequest(HttpListenerRequest request)
        {
            Advanced = request;

            PathInfo = Advanced.RawUrl.Split(new[] { '?' }, 2)[0];
            ContentType = ContentTypes.FromString(Advanced.ContentType);
            HttpMethod = HttpMethods.FromString(Advanced.HttpMethod);

            Name = $"{HttpMethod} {PathInfo}";
            Id = Path.GetRandomFileName().Replace(".", string.Empty).Substring(0,10);
        }
    }
}
