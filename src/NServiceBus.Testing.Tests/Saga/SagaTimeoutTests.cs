namespace NServiceBus.Testing.Tests.Saga
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    class SagaTimeoutTests
    {
        [Test]
        public void ShouldInvokeAllRegisteredTimeouts()
        {
            Test.Saga<TimeoutSaga>()
                .WhenHandling<TimeoutSaga.StartMessage>()
                .ExpectSend<TimeoutSaga.SendMessage1>()
                .ExpectSend<TimeoutSaga.SendMessage2>()
                .WhenSagaTimesOut();
        }

        [Test]
        public void ShouldOnlyInvokeTimeoutsWithinSpecifiedTimeRange()
        {
            Test.Saga<TimeoutSaga>()
                .WhenHandling<TimeoutSaga.StartMessage>()
                .ExpectSend<TimeoutSaga.SendMessage1>()
                .ExpectNotSend<TimeoutSaga.SendMessage2>()
                .WhenSagaTimesOut(TimeSpan.FromDays(10));
        }

        [Test]
        public void ShouldThrowAnExceptionWhenExpectingTimeoutsWithinASpecifiedTimeRange()
        {
            Assert.Throws<ExpectationException>(() => Test.Saga<TimeoutSaga>()
                .WhenHandling<TimeoutSaga.StartMessage>()
                .ExpectSend<TimeoutSaga.SendMessage1>()
                .ExpectSend<TimeoutSaga.SendMessage2>()
                .WhenSagaTimesOut(TimeSpan.FromDays(10)));
        }
    }

    public class TimeoutSaga : NServiceBus.Saga<TimeoutSaga.TimeoutData>,
        IHandleMessages<TimeoutSaga.StartMessage>,
        IHandleTimeouts<TimeoutSaga.TimeoutMessage1>,
        IHandleTimeouts<TimeoutSaga.TimeoutMessage2>
    {
        public async Task Handle(StartMessage message, IMessageHandlerContext context)
        {
            await RequestTimeout(context, TimeSpan.FromDays(7), new TimeoutMessage1());
            await RequestTimeout(context, DateTime.Now + TimeSpan.FromDays(14), new TimeoutMessage2());
        }

        public Task Timeout(TimeoutMessage1 state, IMessageHandlerContext context)
        {
            return context.Send(new SendMessage1());
        }

        public Task Timeout(TimeoutMessage2 state, IMessageHandlerContext context)
        {
            return context.Send(new SendMessage2());
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<TimeoutData> mapper)
        {
        }

        public class TimeoutData : IContainSagaData
        {
            public Guid Id { get; set; }

            public string Originator { get; set; }

            public string OriginalMessageId { get; set; }
        }

        public class SendMessage1
        {
        }

        public class SendMessage2
        {
        }

        public class TimeoutMessage1
        {
        }

        public class TimeoutMessage2
        {
        }

        public class StartMessage
        {
        }
    }
}