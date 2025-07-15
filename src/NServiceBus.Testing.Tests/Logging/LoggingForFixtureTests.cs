#pragma warning disable IDE1006 // Ignore naming rule for test
namespace NServiceBus.Testing.Tests.Logging
{
    using System;
    using System.IO;
    using NServiceBus.Logging;
    using NUnit.Framework;

    [TestFixture]
    [Parallelizable]
    public class LoggingForFixtureTests
    {
        StringWriter writer;
        IDisposable scope;

        [SetUp]
        public void Setup()
        {
            writer = new StringWriter();

            this.scope = LogManager.Use<TestingLoggerFactory>()
                .BeginScope(writer);
        }

        [Test]
        public void Should_write_first_independent_from_other()
        {
            var Logger = LogManager.GetLogger<LoggingForFixtureTests>();
            Logger.Debug("First");

            StringAssert.Contains("NServiceBus.Testing.Tests.Logging.LoggingForFixtureTests First", writer.ToString());
            StringAssert.DoesNotContain("NServiceBus.Testing.Tests.Logging.LoggingForFixtureTests Second", writer.ToString());
        }

        [Test]
        public void Should_write_second_independent_from_other()
        {
            var Logger = LogManager.GetLogger<LoggingForFixtureTests>();
            Logger.Debug("Second");

            StringAssert.Contains("NServiceBus.Testing.Tests.Logging.LoggingForFixtureTests Second", writer.ToString());
            StringAssert.DoesNotContain("NServiceBus.Testing.Tests.Logging.LoggingForFixtureTests First", writer.ToString());
        }

        [TearDown]
        public void Teardown()
        {
            scope.Dispose();
        }
    }
}