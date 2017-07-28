using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using Grapevine.Common;
using Grapevine.Core;
using Grapevine.Server;

namespace Grapevine.Tests.Unit.Stubs
{
    public class StubContext : IHttpContext
    {
        public IHttpRequest Request { get; set; }
        public IHttpResponse Response { get; set; }
        public IRestServer Server { get; set; }
        public bool WasRespondedTo => Response.ResponseSent;

        public StubContext()
        {
            Request = new StubRequest();
            Response = new StubResponse();
        }
    }

    public class StubRequest : IHttpRequest
    {
        public ContentType ContentType { get; set; } = ContentType.JSON;

        public NameValueCollection Headers { get; } = new NameValueCollection();

        public HttpMethod HttpMethod { get; set; } = HttpMethod.POST;

        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string Name => $"{HttpMethod} {PathInfo}";

        public string PathInfo { get; set; } = "/";

        public Dictionary<string, string> PathParameters { get; set; } = new Dictionary<string, string>();

        public NameValueCollection QueryString { get; set; } = new NameValueCollection();
    }

    public class StubResponse : IHttpResponse
    {
        public Encoding ContentEncoding { get; set; } = Encoding.ASCII;

        public ContentType ContentType { get; set; } = ContentType.JSON;

        public NameValueCollection Headers { get; set; } = new NameValueCollection();

        public bool ResponseSent { get; set; }

        public HttpStatusCode StatusCode { get; set; } = HttpStatusCode.Ok;

        public string StatusDescription => StatusCode.ConvertToString();

        public void AddHeader(string name, string value)
        {
            Headers[name] = value;
        }

        public void SendResponse(byte[] contents)
        {
            ResponseSent = true;
        }
    }
}
