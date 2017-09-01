namespace Grapevine.Core.Json
{
    public static class GrapevineJsonManager
    {
        static GrapevineJsonManager()
        {
            Provider = new JsonNotImplementedProvider();
        }

        /// <summary>
        /// The logging provider used for logging in Grapevine.
        /// </summary>
        public static IGrapevineJsonProvider Provider { get; set; }
    }
}
