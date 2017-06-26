using System;
using Grapevine.Core.Logging;
using NLog;

namespace Grapevine.Samples
{
    public class NLogLoggingProvider : IGrapevineLoggingProvider
    {
        public GrapevineLogger CreateLogger(string name)
        {
            return new NLogLogger(name);
        }
    }

    public class NLogLogger : GrapevineLogger
    {
        private readonly Logger _log;

        internal NLogLogger(string name)
        {
            _log = LogManager.GetLogger(name);
        }

        public override bool IsEnabled(GrapevineLogLevel level)
        {
            return _log.IsEnabled(ToNLogLogLevel(level));
        }

        public override void Log(GrapevineLogLevel level, string requestId, string msg, Exception exception = null)
        {
            var ev = new LogEventInfo(ToNLogLogLevel(level), "", msg);
            if (exception != null) ev.Exception = exception;
            if (!string.IsNullOrWhiteSpace(requestId)) ev.Properties["RequestId"] = requestId;
            _log.Log(ev);
        }

        private static LogLevel ToNLogLogLevel(GrapevineLogLevel level)
        {
            switch (level)
            {
                case GrapevineLogLevel.Trace:
                    return LogLevel.Trace;
                case GrapevineLogLevel.Debug:
                    return LogLevel.Debug;
                case GrapevineLogLevel.Info:
                    return LogLevel.Info;
                case GrapevineLogLevel.Warn:
                    return LogLevel.Warn;
                case GrapevineLogLevel.Error:
                    return LogLevel.Error;
                case GrapevineLogLevel.Fatal:
                    return LogLevel.Fatal;
                default:
                    throw new ArgumentOutOfRangeException(nameof(level));
            }
        }
    }
}
