namespace NServiceBus.Testing.Tests.Logging;

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

        scope = LogManager.Use<TestingLoggerFactory>()
            .BeginScope(writer);
    }

    [Test]
    public void Should_write_first_independent_from_other()
    {
        var logger = LogManager.GetLogger<LoggingForFixtureTests>();
        logger.Debug("First");

        Assert.That(writer.ToString(), Does.Contain("NServiceBus.Testing.Tests.Logging.LoggingForFixtureTests First"));
        Assert.That(writer.ToString(), Does.Not.Contain("NServiceBus.Testing.Tests.Logging.LoggingForFixtureTests Second"));
    }

    [Test]
    public void Should_write_second_independent_from_other()
    {
        var logger = LogManager.GetLogger<LoggingForFixtureTests>();
        logger.Debug("Second");

        Assert.That(writer.ToString(), Does.Contain("NServiceBus.Testing.Tests.Logging.LoggingForFixtureTests Second"));
        Assert.That(writer.ToString(), Does.Not.Contain("NServiceBus.Testing.Tests.Logging.LoggingForFixtureTests First"));
    }

    [TearDown]
    public void Teardown()
    {
        scope.Dispose();
        writer.Dispose();
    }
}