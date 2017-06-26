using System;
using System.Text;

namespace Grapevine.Logging
{
    public class ConsoleLoggingProvider : IGrapevineLoggingProvider
    {
        private readonly GrapevineLogLevel _minLevel;
        private readonly bool _printLevel;
        private readonly bool _printRequestId;

        /// <summary>
        /// Constructs a new <see cref="ConsoleLoggingProvider"/>
        /// </summary>
        /// <param name="minLevel">Only messages of this level of higher will be logged</param>
        /// <param name="printLevel">If true, will output the log level (e.g. WARN). Defaults to false.</param>
        /// <param name="printRequestId">If true, will output the internal request id. Defaults to false.</param>
        public ConsoleLoggingProvider(GrapevineLogLevel minLevel = GrapevineLogLevel.Info, bool printLevel = false, bool printRequestId = false)
        {
            _minLevel = minLevel;
            _printLevel = printLevel;
            _printRequestId = printRequestId;
        }

        /// <summary>
        /// Creates a new <see cref="ConsoleLogger"/> instance of the given name.
        /// </summary>
        public GrapevineLogger CreateLogger(string name)
        {
            return new ConsoleLogger(_minLevel, _printLevel, _printRequestId);
        }
    }

    public class ConsoleLogger : GrapevineLogger
    {
        private readonly GrapevineLogLevel _minLevel;
        private readonly bool _printLevel;
        private readonly bool _printRequestId;

        internal ConsoleLogger(GrapevineLogLevel minLevel, bool printLevel, bool printRequestId)
        {
            _minLevel = minLevel;
            _printLevel = printLevel;
            _printRequestId = printRequestId;
        }

        public override bool IsEnabled(GrapevineLogLevel level)
        {
            return level >= _minLevel;
        }

        public override void Log(GrapevineLogLevel level, string requestId, string msg, Exception exception = null)
        {
            if (!IsEnabled(level)) return;

            var sb = new StringBuilder();
            if (_printLevel)
            {
                sb.Append(level.ToString().ToUpper());
                sb.Append(' ');
            }

            if (_printRequestId && !string.IsNullOrWhiteSpace(requestId))
            {
                sb.Append("[");
                sb.Append(requestId);
                sb.Append("] ");
            }

            sb.AppendLine(msg);

            if (exception != null)
            {
                sb.AppendLine(exception.ToString());
            }

            Console.Error.WriteLine(sb.ToString());
        }
    }
}