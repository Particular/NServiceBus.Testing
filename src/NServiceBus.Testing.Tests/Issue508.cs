namespace NServiceBus.Testing.Tests
{
    using System;
    using System.Threading.Tasks;
    using MyMessages;
    using NUnit.Framework;

    [TestFixture]
    public class Issue508
    {
        [Test]
        public void TimeoutInThePast()
        {
            var expected = DateTime.UtcNow.AddDays(-3);
            var message = new TheMessage { TimeoutAt = expected };

            Test.Saga<TheSaga>()
                .ExpectTimeoutToBeSetAt<TheTimeout>((m, at) => at == expected)
                .When((s, c) => s.Handle(message, c));
        }

        [Test]
        public void TimeoutInThePastWithSendOnTimeout()
        {
            var message = new TheMessage { TimeoutAt = DateTime.UtcNow.AddDays(-3) };

            Test
                .Saga<TheSaga>()
                .ExpectTimeoutToBeSetAt<TheTimeout>((m, at) => true)
                .When((s, c) => s.Handle(message, c))
                .ExpectSend<TheMessageSentAtTimeout>()
                .WhenSagaTimesOut();
        }

        [Test]
        public void TimeoutInTheFuture()
        {
            var expected = DateTime.UtcNow.AddDays(3);
            var message = new TheMessage { TimeoutAt = expected };

            Test
                .Saga<TheSaga>()
                .ExpectTimeoutToBeSetAt<TheTimeout>((m, at) => at == expected)
                .When((s, c) => s.Handle(message, c));
        }
    }

    public class TheSaga : NServiceBus.Saga<TheData>,
                           IAmStartedByMessages<TheMessage>,
                           IHandleTimeouts<TheTimeout>
    {
        public Task Handle(TheMessage message, IMessageHandlerContext context)
        {
            return RequestTimeout<TheTimeout>(context, message.TimeoutAt);
        }

        public Task Timeout(TheTimeout state, IMessageHandlerContext context)
        {
            context.Send(new TheMessageSentAtTimeout());
            MarkAsComplete();

            return Task.FromResult(0);
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<TheData> mapper)
        {
        }
    }

    public class TheData : IContainSagaData
    {
        public Guid Id { get; set; }
        public string Originator { get; set; }
        public string OriginalMessageId { get; set; }
    }

    namespace MyMessages
    {
        public class TheMessage : IMessage
        {
            public DateTime TimeoutAt { get; set; }
        }

        public class TheTimeout : IMessage
        {
        }

        public class TheMessageSentAtTimeout : IMessage
        {
        }
    }
}