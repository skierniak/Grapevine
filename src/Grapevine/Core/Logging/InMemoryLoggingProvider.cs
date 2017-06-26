using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Grapevine.Logging
{
    public class InMemoryLoggingProvider : IGrapevineLoggingProvider
    {
        public GrapevineLogger CreateLogger(string name)
        {
            return InMemoryLogger.GetLogger(name);
        }
    }

    public class InMemoryLogger : GrapevineLogger
    {
        private static readonly ConcurrentDictionary<string, InMemoryLogger> CreatedLoggers;

        static InMemoryLogger()
        {
            CreatedLoggers = new ConcurrentDictionary<string, InMemoryLogger>();
        }

        public static InMemoryLogger GetLogger(string name)
        {
            return CreatedLoggers.GetOrAdd(name, s => new InMemoryLogger());
        }

        public static void RemoveLogger(string name)
        {
            InMemoryLogger logger;
            CreatedLoggers.TryRemove(name, out logger);
        }

        public List<LogEvent> Logs { get; }

        protected InMemoryLogger()
        {
            Logs = new List<LogEvent>();
        }

        public override bool IsEnabled(GrapevineLogLevel level)
        {
            return true;
        }

        public override void Log(GrapevineLogLevel level, string requestId, string msg, Exception exception = null)
        {
            Logs.Add(new LogEvent { Level = level, RequestId = requestId, Message = msg, Exception = exception });
        }
    }

    public class LogEvent
    {
        public GrapevineLogLevel Level { get; set; }
        public string RequestId { get; set; }
        public string Message { get; set; }
        public Exception Exception { get; set; }
    }
}