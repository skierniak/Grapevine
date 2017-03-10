using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Grapevine.Common
{
    public static class HttpMethods
    {
        private static readonly ConcurrentDictionary<string, int> Lookup;

        static HttpMethods()
        {
            Lookup = new ConcurrentDictionary<string, int>();

            foreach (var val in Enum.GetValues(typeof(HttpMethod)).Cast<HttpMethod>())
            {
                var key = val.ToString();
                Lookup[key] = (int)val;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the HttpMethods are equal OR one of them is HttpMethod.ALL
        /// </summary>
        /// <param name="httpMethod"></param>
        /// <param name="other"></param>
        /// <returns>bool</returns>
        public static bool IsEquivalentTo(this HttpMethod httpMethod, HttpMethod other)
        {
            return httpMethod == HttpMethod.ALL || other == HttpMethod.ALL || httpMethod == other;
        }

        /// <summary>
        ///  Returns an HttpMethod value for the method string provided
        /// </summary>
        /// <param name="method"></param>
        /// <returns></returns>
        public static HttpMethod FromString(string method)
        {
            var ucMethod = method.ToUpper();
            return (Lookup.ContainsKey(ucMethod)) ? (HttpMethod)Lookup[ucMethod] : 0;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum HttpMethod
    {
        UNKNOWN,
        ALL,
        CONNECT,
        COPY,
        DELETE,
        GET,
        HEAD,
        LINK,
        LOCK,
        OPTIONS,
        PATCH,
        POST,
        PROPFIND,
        PURGE,
        PUT,
        TRACE,
        UNLINK,
        UNLOCK
    }
}
