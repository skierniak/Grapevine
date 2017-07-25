using System;
using System.Collections.Generic;
using System.Text;

namespace Grapevine.Core.Logging
{
    public class ConsoleLoggingProvider : IGrapevineLoggingProvider
    {
        private readonly GrapevineLogLevel _minLevel;

        /// <summary>
        /// Constructs a new <see cref="ConsoleLoggingProvider"/>
        /// </summary>
        /// <param name="minLevel">Only messages of this level of higher will be logged</param>
        public ConsoleLoggingProvider(GrapevineLogLevel minLevel = GrapevineLogLevel.Info)
        {
            _minLevel = minLevel;
        }

        /// <summary>
        /// Creates a new <see cref="ConsoleLogger"/> instance of the given name.
        /// </summary>
        public GrapevineLogger CreateLogger(string name)
        {
            return new ConsoleLogger(_minLevel);
        }
    }

    public class ConsoleLogger : GrapevineLogger
    {
        private readonly GrapevineLogLevel _minLevel;

        /// <summary>
        /// String defining the way the date and time should be formtted when logged
        /// </summary>
        protected internal string DateFormat => @"M/d/yyyy hh:mm:ss tt";

        internal ConsoleLogger(GrapevineLogLevel minLevel)
        {
            _minLevel = minLevel;
        }

        public override bool IsEnabled(GrapevineLogLevel level)
        {
            return level >= _minLevel;
        }

        public override void Log(GrapevineLogLevel level, string requestId, string message, Exception exception = null)
        {
            if (!IsEnabled(level)) return;

            var now = DateTime.Now.ToString(DateFormat);
            var lvl = level.ToString().ToUpper();
            var msg = exception == null ? message : $"{message}:{exception.Message}{Environment.NewLine}{exception.StackTrace}";
            var rid = requestId ?? "---";

            Console.Error.WriteLine($"{now}\t{lvl}\t[{rid}] {msg}");
        }
    }
}