using Grapevine.Core.Logging;
using Shouldly;
using Xunit;

namespace Grapevine.Tests.Unit.Core.Logging
{
    public class InMemoryLoggingProviderFacts
    {
        [Fact]
        public void LogsAllLevels()
        {
            const string logid = "logs-all-levels";
            var logger = InMemoryLogger.GetLogger(logid);

            logger.ShouldSatisfyAllConditions
            (
                () => logger.IsEnabled(GrapevineLogLevel.Trace).ShouldBeTrue(),
                () => logger.IsEnabled(GrapevineLogLevel.Debug).ShouldBeTrue(),
                () => logger.IsEnabled(GrapevineLogLevel.Info).ShouldBeTrue(),
                () => logger.IsEnabled(GrapevineLogLevel.Warn).ShouldBeTrue(),
                () => logger.IsEnabled(GrapevineLogLevel.Error).ShouldBeTrue(),
                () => logger.IsEnabled(GrapevineLogLevel.Fatal).ShouldBeTrue()
            );

            InMemoryLogger.RemoveLogger(logid);
        }

        [Fact]
        public void RemovesLogger()
        {
            const string logid = "removeable-logger";
            var logger = InMemoryLogger.GetLogger(logid);

            logger.Log(GrapevineLogLevel.Debug, "1234", "message");
            logger.Logs.Count.ShouldBe(1);

            logger = InMemoryLogger.GetLogger(logid);
            logger.Logs.Count.ShouldBe(1);

            InMemoryLogger.RemoveLogger(logid);

            logger = InMemoryLogger.GetLogger(logid);
            logger.Logs.Count.ShouldBe(0);
        }
    }
}
