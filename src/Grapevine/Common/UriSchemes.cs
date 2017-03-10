using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Grapevine.Common
{
    public static class UriSchemes
    {
        private static readonly ConcurrentDictionary<UriScheme, string> Lookup;

        static UriSchemes()
        {
            Lookup = new ConcurrentDictionary<UriScheme, string>();

            foreach (var uriScheme in Enum.GetValues(typeof(UriScheme)).Cast<UriScheme>())
            {
                Lookup[uriScheme] = uriScheme.ToString().ToLower();
            }
        }

        /// <summary>
        /// Returns the lowercase representation of the uri scheme value
        /// </summary>
        /// <param name="scheme"></param>
        /// <returns></returns>
        public static string ToScheme(this UriScheme scheme)
        {
            return Lookup[scheme];
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public enum UriScheme
    {
        Http,
        File,
        Ftp,
        Gopher,
        Https,
        MailTo,
        News
    }
}
