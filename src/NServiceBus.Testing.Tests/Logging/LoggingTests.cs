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
            using (loggerFactory.Use(firstStringWriter))
            {
                var firstInstance = new SomeClassThatUsesStaticLogger();
                firstInstance.DoSomething();
            }

            var secondStringWriter = new StringWriter();
            using (loggerFactory.Use(secondStringWriter))
            {
                var secondInstance = new SomeClassThatUsesStaticLogger();
                secondInstance.DoSomething();
            }

            var firstLogString = firstStringWriter.ToString();
            var secondLogString = secondStringWriter.ToString();

            Assert.AreNotEqual(firstLogString, secondLogString);
            StringAssert.Contains("NServiceBus.Testing.Tests.Logging.LoggingTests+SomeClassThatUsesStaticLogger 0", firstLogString);
            StringAssert.DoesNotContain("NServiceBus.Testing.Tests.Logging.LoggingTests+SomeClassThatUsesStaticLogger 1", firstLogString);
            StringAssert.Contains("NServiceBus.Testing.Tests.Logging.LoggingTests+SomeClassThatUsesStaticLogger 1", secondLogString);
            StringAssert.DoesNotContain("NServiceBus.Testing.Tests.Logging.LoggingTests+SomeClassThatUsesStaticLogger 0", secondLogString);
        }

        [Test]
        public void Scoped_Loglevel_should_be_honored()
        {
            var firstStringWriter = new StringWriter();
            var loggerFactory = LogManager.Use<TestingLoggerFactory>();
            using (loggerFactory.Use(firstStringWriter, LogLevel.Warn))
            {
                var firstInstance = new SomeClassThatUsesStaticLogger();
                firstInstance.DoSomething();
            }

            var secondStringWriter = new StringWriter();
            using (loggerFactory.Use(secondStringWriter))
            {
                var secondInstance = new SomeClassThatUsesStaticLogger();
                secondInstance.DoSomething();
            }

            var firstLogString = firstStringWriter.ToString();
            var secondLogString = secondStringWriter.ToString();

            Assert.AreNotEqual(firstLogString, secondLogString);
            Assert.IsEmpty(firstLogString);
            StringAssert.Contains("NServiceBus.Testing.Tests.Logging.LoggingTests+SomeClassThatUsesStaticLogger 1", secondLogString);
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
            using (loggerFactory.Use(secondStringWriter))
            {
                var secondInstance = new SomeClassThatUsesStaticLogger();
                secondInstance.DoSomething();
            }
            
            var globalLogString = globalWriter.ToString();
            var scopedLogString = secondStringWriter.ToString();

            Assert.AreNotEqual(globalLogString, scopedLogString);
            StringAssert.Contains("NServiceBus.Testing.Tests.Logging.LoggingTests+SomeClassThatUsesStaticLogger 0", globalLogString);
            StringAssert.DoesNotContain("NServiceBus.Testing.Tests.Logging.LoggingTests+SomeClassThatUsesStaticLogger 1", globalLogString);
            StringAssert.Contains("NServiceBus.Testing.Tests.Logging.LoggingTests+SomeClassThatUsesStaticLogger 1", scopedLogString);
            StringAssert.DoesNotContain("NServiceBus.Testing.Tests.Logging.LoggingTests+SomeClassThatUsesStaticLogger 0", scopedLogString);
        }

        [Test]
        public void Scope_cannot_be_nested()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                var loggerFactory = LogManager.Use<TestingLoggerFactory>();
                using (loggerFactory.Use(new StringWriter()))
                using (loggerFactory.Use(new StringWriter()))
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