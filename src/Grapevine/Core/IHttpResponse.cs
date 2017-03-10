using System.IO;
using System.IO.Compression;
using Grapevine.Common;

namespace Grapevine.Core
{
    public interface IHttpResponse<out TResponse>
    {
        TResponse Advanced { get; }
    }

    public interface IInboundHttpResponse
    {

    }

    public interface IInboundHttpResponse<out TResponse> : IInboundHttpResponse, IHttpResponse<TResponse>
    {
        
    }

    public interface IOutboundHttpResponse
    {
        bool ResponseSent { get; }

        void AddHeader(string name, string value);

        void SendResponse(byte[] contents);
    }

    public interface IOutboundHttpResponse<out TResponse> : IOutboundHttpResponse, IHttpResponse<TResponse>
    {
        
    }

    /// <summary>
    /// Represents a response to a request being handled by an HttpListener object
    /// </summary>
    public class OutboundHttpResponse : IOutboundHttpResponse<System.Net.HttpListenerResponse>
    {
        public System.Net.HttpListenerResponse Advanced { get; }

        public bool ResponseSent { get; protected internal set; }

        public bool EnableCompress { get; set; }

        public OutboundHttpResponse(System.Net.HttpListenerResponse response)
        {
            Advanced = response;
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

    public static class OutboundHttpResponseExtensions
    {
        public static void SendResponse(this IOutboundHttpResponse response, HttpStatusCode statusCode)
        {
            
        }

        public static void SendResponse(this IOutboundHttpResponse response, Stream stream)
        {

        }
    }
}