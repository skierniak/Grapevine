using System;

namespace Grapevine.Logging
{
    public abstract class GrapevineLogger
    {
        public abstract bool IsEnabled(GrapevineLogLevel level);
        public abstract void Log(GrapevineLogLevel level, string requestId, string msg, Exception exception = null);

        internal void Trace(string msg, string requestId = null) { Log(GrapevineLogLevel.Trace, requestId, msg); }
        internal void Debug(string msg, string requestId = null) { Log(GrapevineLogLevel.Debug, requestId, msg); }
        internal void Info(string msg, string requestId = null) { Log(GrapevineLogLevel.Info, requestId, msg); }
        internal void Warn(string msg, string requestId = null) { Log(GrapevineLogLevel.Warn, requestId, msg); }
        internal void Error(string msg, string requestId = null) { Log(GrapevineLogLevel.Error, requestId, msg); }
        internal void Fatal(string msg, string requestId = null) { Log(GrapevineLogLevel.Fatal, requestId, msg); }

        internal void Trace(string msg, Exception ex, string requestId = null) { Log(GrapevineLogLevel.Trace, requestId, msg, ex); }
        internal void Debug(string msg, Exception ex, string requestId = null) { Log(GrapevineLogLevel.Debug, requestId, msg, ex); }
        internal void Info(string msg, Exception ex, string requestId = null) { Log(GrapevineLogLevel.Info, requestId, msg, ex); }
        internal void Warn(string msg, Exception ex, string requestId = null) { Log(GrapevineLogLevel.Warn, requestId, msg, ex); }
        internal void Error(string msg, Exception ex, string requestId = null) { Log(GrapevineLogLevel.Error, requestId, msg, ex); }
        internal void Fatal(string msg, Exception ex, string requestId = null) { Log(GrapevineLogLevel.Fatal, requestId, msg, ex); }
    }
}
