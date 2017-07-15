using System.Collections.Specialized;
using System.Net;
using System.Text;
using Grapevine.Common;
using HttpStatusCode = Grapevine.Common.HttpStatusCode;

namespace Grapevine.Core
{
    public interface IHttpResponse
    {
        /// <summary>
        /// Gets or sets the Encoding for this response's OutputStream
        /// </summary>
        Encoding ContentEncoding { get; set; }

        /// <summary>
        /// Gets or sets the MIME type of the content returned
        /// </summary>
        ContentType ContentType { get; set; }

        /// <summary>
        /// Gets or sets the collection of header name/value pairs returned by the server
        /// </summary>
        NameValueCollection Headers { get; set; }

        bool ResponseSent { get; set; }

        HttpStatusCode StatusCode { get; set; }

        string StatusDescription { get; }

        void AddHeader(string name, string value);

        void SendResponse(byte[] contents);
    }

    public class HttpResponse : IHttpResponse
    {
        public static int DefaultHoursToExpire = 23;

        public HttpListenerResponse Advanced { get; protected internal set; }

        public Encoding ContentEncoding { get; set; }

        public ContentType ContentType { get; set; }

        public NameValueCollection Headers { get; set; }

        public bool ResponseSent { get; set; }

        public HttpStatusCode StatusCode { get; set; }

        public string StatusDescription => Advanced.StatusDescription;

        protected internal HttpResponse(HttpListenerResponse response)
        {
            Advanced = response;
            ContentEncoding = Encoding.ASCII;
        }

        public void AddHeader(string name, string value)
        {
            Advanced.AddHeader(name, value);
        }

        public void SendResponse(byte[] contents)
        {
            /*
             * https://en.wikipedia.org/wiki/HTTP_compression
             * This is where compression used to happen. I think I want to move this elsewhere
             * so that multiple compression modes can be supported, but I need to do additional
             * research on this first.
             * 
             * The original method is intact below, for reference.
            if (RequestHeaders.AllKeys.Contains("Accept-Encoding") && RequestHeaders["Accept-Encoding"].Contains("gzip") && contents.Length > 1024)
            {
                using (var ms = new MemoryStream())
                {
                    using (var zip = new GZipStream(ms, CompressionMode.Compress))
                    {
                        zip.Write(contents, 0, contents.Length);
                    }
                    contents = ms.ToArray();
                }
                Advanced.Headers["Content-Encoding"] = "gzip";
            }
            */

            Advanced.ContentLength64 = contents.Length;
            Advanced.OutputStream.Write(contents, 0, contents.Length);
            Advanced.OutputStream.Close();
            Advanced.Close();

            ResponseSent = true;
        }
    }
}