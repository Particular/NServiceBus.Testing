namespace NServiceBus.Testing.Tests.Logging
{
    using System;
    using System.IO;
    using NServiceBus.Logging;
    using NUnit.Framework;

    [TestFixture]
    public class LoggingTests
    {
        [TearDown]
        public void Teardown()
        {
            SomeClassThatUsesStaticLogger.Reset();
        }

        [Test]
        public void Scoped_Writer_should_be_honored()
        {
            var firstStringWriter = new StringWriter();
            var loggerFactory = LogManager.Use<TestingLoggerFactory>();
            using (loggerFactory.BeginScope(firstStringWriter))
            {
                var firstInstance = new SomeClassThatUsesStaticLogger();
                firstInstance.DoSomething();
            }

            var secondStringWriter = new StringWriter();
            using (loggerFactory.BeginScope(secondStringWriter))
            {
                var secondInstance = new SomeClassThatUsesStaticLogger();
                secondInstance.DoSomething();
            }

            var firstLogString = firstStringWriter.ToString();
            var secondLogString = secondStringWriter.ToString();

            Assert.That(secondLogString, Is.Not.EqualTo(firstLogString));
            Assert.That(firstLogString, Does.Contain("NServiceBus.Testing.Tests.Logging.LoggingTests+SomeClassThatUsesStaticLogger 0"));
            Assert.That(firstLogString, Does.Not.Contain("NServiceBus.Testing.Tests.Logging.LoggingTests+SomeClassThatUsesStaticLogger 1"));
            Assert.That(secondLogString, Does.Contain("NServiceBus.Testing.Tests.Logging.LoggingTests+SomeClassThatUsesStaticLogger 1"));
            Assert.That(secondLogString, Does.Not.Contain("NServiceBus.Testing.Tests.Logging.LoggingTests+SomeClassThatUsesStaticLogger 0"));
        }

        [Test]
        public void Scoped_Loglevel_should_be_honored()
        {
            var firstStringWriter = new StringWriter();
            var loggerFactory = LogManager.Use<TestingLoggerFactory>();
            using (loggerFactory.BeginScope(firstStringWriter, LogLevel.Warn))
            {
                var firstInstance = new SomeClassThatUsesStaticLogger();
                firstInstance.DoSomething();
            }

            var secondStringWriter = new StringWriter();
            using (loggerFactory.BeginScope(secondStringWriter))
            {
                var secondInstance = new SomeClassThatUsesStaticLogger();
                secondInstance.DoSomething();
            }

            var firstLogString = firstStringWriter.ToString();
            var secondLogString = secondStringWriter.ToString();

            Assert.Multiple(() =>
            {
                Assert.That(secondLogString, Is.Not.EqualTo(firstLogString));
                Assert.That(firstLogString, Is.Empty);
            });
            Assert.That(secondLogString, Does.Contain("NServiceBus.Testing.Tests.Logging.LoggingTests+SomeClassThatUsesStaticLogger 1"));
        }

        [Test]
        public void Global_Writer_should_be_honored()
        {
            var loggerFactory = LogManager.Use<TestingLoggerFactory>();
            var globalWriter = new StringWriter();
            loggerFactory.WriteTo(globalWriter);

            var firstInstance = new SomeClassThatUsesStaticLogger();
            firstInstance.DoSomething();

            var secondStringWriter = new StringWriter();
            using (loggerFactory.BeginScope(secondStringWriter))
            {
                var secondInstance = new SomeClassThatUsesStaticLogger();
                secondInstance.DoSomething();
            }

            var globalLogString = globalWriter.ToString();
            var scopedLogString = secondStringWriter.ToString();

            Assert.That(scopedLogString, Is.Not.EqualTo(globalLogString));
            Assert.That(globalLogString, Does.Contain("NServiceBus.Testing.Tests.Logging.LoggingTests+SomeClassThatUsesStaticLogger 0"));
            Assert.That(globalLogString, Does.Not.Contain("NServiceBus.Testing.Tests.Logging.LoggingTests+SomeClassThatUsesStaticLogger 1"));
            Assert.That(scopedLogString, Does.Contain("NServiceBus.Testing.Tests.Logging.LoggingTests+SomeClassThatUsesStaticLogger 1"));
            Assert.That(scopedLogString, Does.Not.Contain("NServiceBus.Testing.Tests.Logging.LoggingTests+SomeClassThatUsesStaticLogger 0"));
        }

        [Test]
        public void Scope_cannot_be_nested()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var loggerFactory = LogManager.Use<TestingLoggerFactory>();
                using (loggerFactory.BeginScope(new StringWriter()))
                using (loggerFactory.BeginScope(new StringWriter()))
                {
                }
            });
        }

        [Test]
        public void NoScope_does_work()
        {
            LogManager.Use<TestingLoggerFactory>();

            var secondInstance = new SomeClassThatUsesStaticLogger();
            secondInstance.DoSomething();
        }

        class SomeClassThatUsesStaticLogger
        {
            public SomeClassThatUsesStaticLogger()
            {
                InstanceCounter = instanceCounter++;
            }

            public int InstanceCounter { get; }

            public void DoSomething()
            {
                Logger.Debug(InstanceCounter.ToString());
            }

            public static void Reset()
            {
                instanceCounter = 0;
            }

            static int instanceCounter;

            static ILog Logger = LogManager.GetLogger<SomeClassThatUsesStaticLogger>();
        }
    }
}