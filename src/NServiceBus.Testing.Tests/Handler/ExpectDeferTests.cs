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
                .ExpectDefer<TestMessage>((m, t) => t == timespan)
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldAssertDeferWasCalledWithDateTime()
        {
            var datetime = DateTime.UtcNow;
            Test.Handler<DeferringDateTimeHandler>()
                .WithExternalDependencies(h => h.Defer = datetime)
                .ExpectDefer<TestMessage>((m, t) => t == datetime)
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldFailAssertingDeferWasCalledWithTimeSpan()
        {
            var timespan = TimeSpan.FromMinutes(10);
            var exception = Assert.Throws<ExpectationException>(() => Test.Handler<EmptyHandler>()
                .ExpectDefer<TestMessage>((m, t) => t == timespan)
                .OnMessage<TestMessage>());

            Assert.AreEqual($"Expected a message of type {nameof(TestMessage)} to be deferred, but no message matching your constraints was deferred.", exception.Message);
        }

        [Test]
        public void ShouldFailAssertingDeferWasCalledWithDateTime()
        {
            var datetime = DateTime.UtcNow;
            var exception = Assert.Throws<ExpectationException>(() => Test.Handler<EmptyHandler>()
                .ExpectDefer<TestMessage>((m, t) => t == datetime)
                .OnMessage<TestMessage>());

            Assert.AreEqual($"Expected a message of type {nameof(TestMessage)} to be deferred, but no message matching your constraints was deferred.", exception.Message);
        }

        [Test]
        public void ShouldAssertDeferWasNotCalledWithTimeSpan()
        {
            var timespan = TimeSpan.FromMinutes(10);
            Test.Handler<EmptyHandler>()
                .ExpectNotDefer<TestMessage>((m, t) => t == timespan)
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldAssertDeferWasNotCalledWithDateTime()
        {
            var datetime = DateTime.UtcNow;
            Test.Handler<EmptyHandler>()
                .ExpectNotDefer<TestMessage>((m, t) => t == datetime)
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldFailAssertingDeferWasNotCalledWithTimeSpan()
        {
            var timespan = TimeSpan.FromMinutes(10);
            var exception = Assert.Throws<ExpectationException>(() => Test.Handler<DeferringTimeSpanHandler>()
                .WithExternalDependencies(h => h.Defer = timespan)
                .ExpectNotDefer<TestMessage>((m, t) => t == timespan)
                .OnMessage<TestMessage>());

            Assert.AreEqual($"Expected no message of type {nameof(TestMessage)} to be deferred, but a message matching your constraints was deferred.", exception.Message);
        }

        [Test]
        public void ShouldFailAssertingDeferWasNotCalledWithDateTime()
        {
            var datetime = DateTime.UtcNow;
            var exception = Assert.Throws<ExpectationException>(() => Test.Handler<DeferringDateTimeHandler>()
                .WithExternalDependencies(h => h.Defer = datetime)
                .ExpectNotDefer<TestMessage>((m, t) => t == datetime)
                .OnMessage<TestMessage>());

            Assert.AreEqual($"Expected no message of type {nameof(TestMessage)} to be deferred, but a message matching your constraints was deferred.", exception.Message);
        }

        public class DeferringDateTimeHandler : IHandleMessages<TestMessage>
        {
            public DateTime Defer { get; set; }

            public Task Handle(TestMessage message, IMessageHandlerContext context)
            {
                var sendOptions = new SendOptions();
                sendOptions.DoNotDeliverBefore(Defer);

                return context.Send(message, sendOptions);
            }
        }

        public class DeferringTimeSpanHandler : IHandleMessages<TestMessage>
        {
            public TimeSpan Defer { get; set; }

            public Task Handle(TestMessage message, IMessageHandlerContext context)
            {
                var sendOptions = new SendOptions();
                sendOptions.DelayDeliveryWith(Defer);

                return context.Send(message, sendOptions);
            }
        }
    }
}