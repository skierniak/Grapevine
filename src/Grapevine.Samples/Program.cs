using Grapevine.Core.Logging;

namespace Grapevine.Samples
{
    public class Program
    {
        public static void Main(string[] args)
        {
            GrapevineLogManager.Provider = new NLogLoggingProvider();
        }
    }
}
