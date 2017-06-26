using System;

namespace Grapevine.Core.Logging
{
    public abstract class GrapevineLogger
    {
        public abstract bool IsEnabled(GrapevineLogLevel level);
        public abstract void Log(GrapevineLogLevel level, string requestId, string msg, Exception exception = null);

        public void Trace(string msg, string requestId = null) { Log(GrapevineLogLevel.Trace, requestId, msg); }
        public void Debug(string msg, string requestId = null) { Log(GrapevineLogLevel.Debug, requestId, msg); }
        public void Info(string msg, string requestId = null) { Log(GrapevineLogLevel.Info, requestId, msg); }
        public void Warn(string msg, string requestId = null) { Log(GrapevineLogLevel.Warn, requestId, msg); }
        public void Error(string msg, string requestId = null) { Log(GrapevineLogLevel.Error, requestId, msg); }
        public void Fatal(string msg, string requestId = null) { Log(GrapevineLogLevel.Fatal, requestId, msg); }

        public void Trace(string msg, Exception ex, string requestId = null) { Log(GrapevineLogLevel.Trace, requestId, msg, ex); }
        public void Debug(string msg, Exception ex, string requestId = null) { Log(GrapevineLogLevel.Debug, requestId, msg, ex); }
        public void Info(string msg, Exception ex, string requestId = null) { Log(GrapevineLogLevel.Info, requestId, msg, ex); }
        public void Warn(string msg, Exception ex, string requestId = null) { Log(GrapevineLogLevel.Warn, requestId, msg, ex); }
        public void Error(string msg, Exception ex, string requestId = null) { Log(GrapevineLogLevel.Error, requestId, msg, ex); }
        public void Fatal(string msg, Exception ex, string requestId = null) { Log(GrapevineLogLevel.Fatal, requestId, msg, ex); }
    }
}
