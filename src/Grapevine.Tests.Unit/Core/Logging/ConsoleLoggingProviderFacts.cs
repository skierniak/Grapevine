using System;
using System.IO;
using Grapevine.Core.Logging;
using Shouldly;
using Xunit;

namespace Grapevine.Tests.Unit.Core.Logging
{
    public class ConsoleLoggingProviderFacts
    {
        public static string RequestId = Guid.NewGuid().ToString();
        public static string MessageText = "This is the message";
        public static string ExceptionText = "This is the exception";
        public static Exception FakeException = new Exception(ExceptionText);

        [Theory,
            InlineData(GrapevineLogLevel.Debug, GrapevineLogLevel.Trace),
            InlineData(GrapevineLogLevel.Info, GrapevineLogLevel.Debug),
            InlineData(GrapevineLogLevel.Warn, GrapevineLogLevel.Info),
            InlineData(GrapevineLogLevel.Error, GrapevineLogLevel.Warn),
            InlineData(GrapevineLogLevel.Fatal, GrapevineLogLevel.Error)]
        public void IsEnabledReturnsFalseWhenLogLevelIsLower(GrapevineLogLevel minLevel, GrapevineLogLevel targetLevel)
        {
            var logger = new ConsoleLoggingProvider(minLevel).CreateLogger("test");
            logger.IsEnabled(targetLevel).ShouldBeFalse();
        }

        [Fact]
        public void DoesNotLogAtLowerLogLevel()
        {
            var stderr = Console.Error;

            using (var sw = new StringWriter())
            {
                Console.SetError(sw);

                var logger = new ConsoleLoggingProvider(GrapevineLogLevel.Fatal).CreateLogger("test");

                logger.Log(GrapevineLogLevel.Trace, "", "Message Text");

                var result = sw.ToString();
                result.ShouldBeNullOrWhiteSpace();
            }

            Console.SetError(stderr);
        }

        [Fact]
        public void LogsToConsole()
        {
            var stderr = Console.Error;

            using (var sw = new StringWriter())
            {
                Console.SetError(sw);
                var logger = new ConsoleLoggingProvider(GrapevineLogLevel.Trace).CreateLogger("test");

                logger.Log(GrapevineLogLevel.Fatal, RequestId, MessageText);

                var result = sw.ToString();
                result.Contains($"\tFATAL\t[{RequestId}] {MessageText}").ShouldBeTrue();
            }

            Console.SetError(stderr);
        }

        [Fact]
        public void LogsExceptionToConsole()
        {
            var stderr = Console.Error;

            using (var sw = new StringWriter())
            {
                Console.SetError(sw);
                var logger = new ConsoleLoggingProvider(GrapevineLogLevel.Trace).CreateLogger("test");

                logger.Log(GrapevineLogLevel.Fatal, RequestId, MessageText, FakeException);

                var result = sw.ToString();
                result.Contains($"\tFATAL\t[{RequestId}] {MessageText}:{FakeException.Message}{Environment.NewLine}{FakeException.StackTrace}").ShouldBeTrue();
            }

            Console.SetError(stderr);
        }

        [Fact]
        public void LogsToConsoleWithoutRequestId()
        {
            var stderr = Console.Error;

            using (var sw = new StringWriter())
            {
                Console.SetError(sw);
                var logger = new ConsoleLoggingProvider(GrapevineLogLevel.Trace).CreateLogger("test");

                logger.Log(GrapevineLogLevel.Fatal, null, MessageText);

                var result = sw.ToString();
                result.Contains($"\tFATAL\t[---] {MessageText}").ShouldBeTrue();
            }

            Console.SetError(stderr);
        }
    }
}
