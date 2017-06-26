using System;
using System.Reflection;
using Grapevine.Core.Logging;
using Shouldly;
using Xunit;

namespace Grapevine.Tests.Unit.Core.Logging
{
    public class GrapevineLogManagerFacts : IDisposable
    {
        [Fact]
        public void DefaultsToInMemoryLoggingProviderInTest()
        {
            GrapevineLogManager.Provider.GetType().ShouldBe(typeof(InMemoryLoggingProvider));
        }

        [Fact]
        public void ThrowsExceptionWhenChangingProviderAfterProviderHasBeenAccessed()
        {
            GrapevineLogManager.Provider.GetType().ShouldBe(typeof(InMemoryLoggingProvider));
            Should.Throw<InvalidOperationException>(() => GrapevineLogManager.Provider = new ConsoleLoggingProvider());
        }

        [Fact]
        public void CreatesLoggerUsingStringName()
        {
            const string name = "TestThisLogger";
            var logger = (InMemoryLogger) GrapevineLogManager.CreateLogger(name);
            logger.Name.ShouldBe(name);
        }

        [Fact]
        public void CreatesLoggerUsingType()
        {
            var name = GetType().FullName;
            var logger = (InMemoryLogger)GrapevineLogManager.CreateLogger(GetType());
            logger.Name.ShouldBe(name);
        }

        [Fact]
        public void CreatesLoggerUsingGeneric()
        {
            var name = GetType().FullName;
            var logger = (InMemoryLogger)GrapevineLogManager.CreateLogger<GrapevineLogManagerFacts>();
            logger.Name.ShouldBe(name);
        }

        [Fact]
        public void CreatesLoggerUsingCurrentClass()
        {
            var name = GetType().FullName;
            var logger = (InMemoryLogger)GrapevineLogManager.GetCurrentClassLogger();
            logger.Name.ShouldBe(name);
        }

        [Fact]
        public void SwitchesToLogToConsole()
        {
            GrapevineLogManager.LogToConsole();
            GrapevineLogManager.Provider.GetType().ShouldBe(typeof(ConsoleLoggingProvider));
        }

        public void Dispose()
        {
            var type = typeof(GrapevineLogManager);
            type.GetField("_providerRetrieved", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, false);
            type.GetField("_provider", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, new InMemoryLoggingProvider());
        }
    }
}
