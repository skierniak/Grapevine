using System;

namespace Grapevine.Core.Logging
{
    public abstract class GrapevineLogger
    {
        public abstract bool IsEnabled(GrapevineLogLevel level);
        public abstract void Log(GrapevineLogLevel level, string requestId, string message, Exception exception = null);

        public void Trace(string message, string requestId = null) { Log(GrapevineLogLevel.Trace, requestId, message); }
        public void Debug(string message, string requestId = null) { Log(GrapevineLogLevel.Debug, requestId, message); }
        public void Info(string message, string requestId = null) { Log(GrapevineLogLevel.Info, requestId, message); }
        public void Warn(string message, string requestId = null) { Log(GrapevineLogLevel.Warn, requestId, message); }
        public void Error(string message, string requestId = null) { Log(GrapevineLogLevel.Error, requestId, message); }
        public void Fatal(string message, string requestId = null) { Log(GrapevineLogLevel.Fatal, requestId, message); }

        public void Trace(string message, Exception exception, string requestId = null) { Log(GrapevineLogLevel.Trace, requestId, message, exception); }
        public void Debug(string message, Exception exception, string requestId = null) { Log(GrapevineLogLevel.Debug, requestId, message, exception); }
        public void Info(string message, Exception exception, string requestId = null) { Log(GrapevineLogLevel.Info, requestId, message, exception); }
        public void Warn(string message, Exception exception, string requestId = null) { Log(GrapevineLogLevel.Warn, requestId, message, exception); }
        public void Error(string message, Exception exception, string requestId = null) { Log(GrapevineLogLevel.Error, requestId, message, exception); }
        public void Fatal(string message, Exception exception, string requestId = null) { Log(GrapevineLogLevel.Fatal, requestId, message, exception); }
    }
}
