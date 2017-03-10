namespace Grapevine.Logging
{
    public interface IGrapevineLoggingProvider
    {
        /// <summary>
        /// Creates a new IGrapevineLogger instance of the given name
        /// </summary>
        /// <param name="name"></param>
        /// <returns><see cref="GrapevineLogger"/></returns>
        GrapevineLogger CreateLogger(string name);
    }
}
