using System;

namespace Grapevine.Server
{
    /// <summary>
    /// <para>Class attribute for defining a RestResource</para>
    /// <para>Targets: Class</para>
    /// <para>&#160;</para>
    /// <para>A class with the RestResource attribute can be scanned for RestRoute attributed methods</para>
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class RestResource : Attribute
    {
        /// <summary>
        /// This value will be prepended to the PathInfo value on all RestRoutes in the class, defaults to an empty string.
        /// </summary>
        public string BasePath { get; set; }

        public RestResource()
        {
            BasePath = string.Empty;
        }
    }
}
