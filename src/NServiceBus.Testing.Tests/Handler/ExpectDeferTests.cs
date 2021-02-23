namespace NServiceBus.Testing.Tests.Handler
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class ExpectDeferTests
    {
        [Test]
        public void ShouldAssertDeferWasCalledWithTimeSpan()
        {
            var timespan = TimeSpan.FromMinutes(10);
            Test.Handler<DeferringTimeSpanHandler>()
                .WithExternalDependencies(h => h.Defer = timespan)
                .ExpectDefer<ITestMessage>((m, t) => t == timespan)
                .OnMessage<ITestMessage>();
        }

        [Test]
        public void ShouldFailAssertingDeferWasCalledWithTimeSpan()
        {
            var timespan = TimeSpan.FromMinutes(10);
            var exception = Assert.Throws<ExpectationException>(() => Test.Handler<EmptyHandler>()
                .ExpectDefer<ITestMessage>((m, t) => t == timespan)
                .OnMessage<ITestMessage>());

            Assert.AreEqual($"Expected a message of type {nameof(ITestMessage)} to be deferred, but no message matching your constraints was deferred.", exception.Message);
        }

        [Test]
        public void ShouldFailAssertingDeferWasCalledWithDateTime()
        {
            var datetime = DateTimeOffset.UtcNow;
            var exception = Assert.Throws<ExpectationException>(() => Test.Handler<EmptyHandler>()
                .ExpectDefer<ITestMessage>((m, t) => t == datetime)
                .OnMessage<ITestMessage>());

            Assert.AreEqual($"Expected a message of type {nameof(ITestMessage)} to be deferred, but no message matching your constraints was deferred.", exception.Message);
        }

        [Test]
        public void ShouldAssertDeferWasNotCalledWithTimeSpan()
        {
            var timespan = TimeSpan.FromMinutes(10);
            Test.Handler<EmptyHandler>()
                .ExpectNotDefer<ITestMessage>((m, t) => t == timespan)
                .OnMessage<ITestMessage>();
        }

        [Test]
        public void ShouldAssertDeferWasNotCalledWithDateTime()
        {
            var datetime = DateTimeOffset.UtcNow;
            Test.Handler<EmptyHandler>()
                .ExpectNotDefer<ITestMessage>((m, t) => t == datetime)
                .OnMessage<ITestMessage>();
        }

        [Test]
        public void ShouldFailAssertingDeferWasNotCalledWithTimeSpan()
        {
            var timespan = TimeSpan.FromMinutes(10);
            var exception = Assert.Throws<ExpectationException>(() => Test.Handler<DeferringTimeSpanHandler>()
                .WithExternalDependencies(h => h.Defer = timespan)
                .ExpectNotDefer<ITestMessage>((m, t) => t == timespan)
                .OnMessage<ITestMessage>());

            Assert.AreEqual($"Expected no message of type {nameof(ITestMessage)} to be deferred, but a message matching your constraints was deferred.", exception.Message);
        }

        [Test]
        public void ShouldFailAssertingDeferWasNotCalledWithDateTime()
        {
            var datetime = DateTimeOffset.UtcNow;
            var exception = Assert.Throws<ExpectationException>(() => Test.Handler<DeferringDateTimeHandler>()
                .WithExternalDependencies(h => h.Defer = datetime)
                .ExpectNotDefer<ITestMessage>((m, t) => t == datetime)
                .OnMessage<ITestMessage>());

            Assert.AreEqual($"Expected no message of type {nameof(ITestMessage)} to be deferred, but a message matching your constraints was deferred.", exception.Message);
        }

        public class DeferringDateTimeHandler : IHandleMessages<ITestMessage>
        {
            public DateTimeOffset Defer { get; set; }

            public Task Handle(ITestMessage message, IMessageHandlerContext context)
            {
                var sendOptions = new SendOptions();
                sendOptions.DoNotDeliverBefore(Defer);

                return context.Send(message, sendOptions);
            }
        }

        public class DeferringTimeSpanHandler : IHandleMessages<ITestMessage>
        {
            public TimeSpan Defer { get; set; }

            public Task Handle(ITestMessage message, IMessageHandlerContext context)
            {
                var sendOptions = new SendOptions();
                sendOptions.DelayDeliveryWith(Defer);

                return context.Send(message, sendOptions);
            }
        }
    }
}