using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Grapevine.Common
{
    /// <summary>
    /// Provides methods for parsing PathInfo patterns
    /// </summary>
    public static class PatternParser
    {
        private static readonly Regex ParseForParams = new Regex(@"\[(\w+)\]", RegexOptions.IgnoreCase);

        /// <summary>
        /// Returns a list of keys parsed from the specified PathInfo pattern
        /// </summary>
        /// <param name="pathInfo"></param>
        /// <returns>List&lt;string&gt;</returns>
        public static List<string> GeneratePatternKeys(string pathInfo)
        {
            var captured = new List<string>();

            foreach (var val in from Match match in ParseForParams.Matches(pathInfo) select match.Groups[1].Value)
            {
                if (captured.Contains(val)) throw new ArgumentException($"Repeat parameters in path info expression {pathInfo}");
                captured.Add(val);
            }

            return captured;
        }

        /// <summary>
        /// Returns a Regex object that matches the specified PathInfo pattern
        /// </summary>
        /// <param name="pathInfo"></param>
        /// <returns>RegEx</returns>
        public static Regex GenerateRegEx(string pathInfo)
        {
            if (string.IsNullOrWhiteSpace(pathInfo)) return new Regex(@"^.*$");
            if (pathInfo.StartsWith("^")) return new Regex(pathInfo);

            var pattern = new StringBuilder("^");

            pattern.Append(ParseForParams.IsMatch(pathInfo)
                ? ParseForParams.Replace(pathInfo, "([^/]+)")
                : pathInfo);

            if (!pathInfo.EndsWith("$")) pattern.Append("$");

            return new Regex(pattern.ToString());
        }

        /// <summary>
        /// Gets a dictionary of parameter values from the PathInfo of the request
        /// </summary>
        /// <param name="pathinfo"></param>
        /// /// <param name="pattern"></param>
        /// /// <param name="keys"></param>
        /// <returns>Dictionary&lt;string, string&gt;</returns>
        public static Dictionary<string, string> ExtractParams(string pathinfo, Regex pattern, List<string> keys)
        {
            var parsed = new Dictionary<string, string>();
            var idx = 0;

            var matches = pattern.Matches(pathinfo)[0].Groups;
            for (var i = 1; i < matches.Count; i++)
            {
                var key = keys.Count > 0 && keys.Count > idx ? keys[idx] : $"p{idx}";
                parsed.Add(key, matches[i].Value);
                idx++;
            }

            return parsed;
        }
    }
}
