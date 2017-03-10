using System;

namespace Grapevine.Logging
{
    public class NoOpLoggingProvider : IGrapevineLoggingProvider
    {
        public GrapevineLogger CreateLogger(string name)
        {
            return NoOpLogger.Instance;
        }
    }

    public class NoOpLogger : GrapevineLogger
    {
        internal static NoOpLogger Instance = new NoOpLogger();

        internal NoOpLogger() { }

        public override bool IsEnabled(GrapevineLogLevel level)
        {
            return false;
        }

        public override void Log(GrapevineLogLevel level, string requestId, string msg, Exception exception = null)
        {
        }
    }
}
