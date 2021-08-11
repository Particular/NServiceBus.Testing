namespace NServiceBus.Testing.Tests.Saga
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    class WhenSagaTimesOutTests
    {
        [Test]
        public void ShouldInvokeAllRegisteredTimeoutsWhenNoPeriodSpecified()
        {
            Test.Saga<TimeoutSaga>()
                .WhenHandling<TimeoutSaga.StartMessage>()
                .ExpectSend<TimeoutSaga.SendMessage1>()
                .ExpectSend<TimeoutSaga.SendMessage2>()
                .ExpectSend<TimeoutSaga.SendMessageDateTime1>()
                .ExpectSend<TimeoutSaga.SendMessageDateTime2>()
                .WhenSagaTimesOut();
        }

        [Test]
        public void ShouldOnlyInvokeTimeoutsWithinSpecifiedTimeSpan()
        {
            Test.Saga<TimeoutSaga>()
                .WhenHandling<TimeoutSaga.StartMessage>()
                .ExpectSend<TimeoutSaga.SendMessage1>()
                .ExpectNotSend<TimeoutSaga.SendMessage2>()
                .ExpectNotSend<TimeoutSaga.SendMessageDateTime1>()
                .ExpectNotSend<TimeoutSaga.SendMessageDateTime2>()
                .WhenSagaTimesOut(TimeSpan.FromDays(10));
        }

        [Test]
        public void ShouldThrowAnExceptionWhenExpectingTimeoutsWithinASpecifiedTimeSpan()
        {
            Assert.Throws<ExpectationException>(() => Test.Saga<TimeoutSaga>()
                .WhenHandling<TimeoutSaga.StartMessage>()
                .ExpectSend<TimeoutSaga.SendMessage1>()
                .ExpectSend<TimeoutSaga.SendMessage2>()
                .WhenSagaTimesOut(TimeSpan.FromDays(10)));
        }

        [Test]
        public void ShouldOnlyInvokeTimeoutsWithinSpecifiedDateTime()
        {
            Test.Saga<TimeoutSaga>()
                .WhenHandling<TimeoutSaga.StartMessage>()
                .ExpectSend<TimeoutSaga.SendMessageDateTime1>()
                .ExpectNotSend<TimeoutSaga.SendMessageDateTime2>()
                .ExpectNotSend<TimeoutSaga.SendMessage1>()
                .ExpectNotSend<TimeoutSaga.SendMessage2>()
                .WhenSagaTimesOut(new DateTimeOffset(2010, 1, 10, 0, 0, 0, TimeSpan.Zero));
        }

        [Test]
        public void ShouldThrowAnExceptionWhenExpectingTimeoutsWithinASpecifiedDateTime()
        {
            Assert.Throws<ExpectationException>(() => Test.Saga<TimeoutSaga>()
                .WhenHandling<TimeoutSaga.StartMessage>()
                .ExpectSend<TimeoutSaga.SendMessageDateTime1>()
                .ExpectSend<TimeoutSaga.SendMessageDateTime2>()
                .WhenSagaTimesOut(new DateTimeOffset(2010, 1, 10, 0, 0, 0, TimeSpan.Zero)));
        }

        [Test]
        public void ShouldInvokeAllRegisteredTimeouts()
        {
            Test.Saga<MultiTimeoutSaga>()
                .WhenHandling<MultiTimeoutSaga.RequestTimeout1>()
                .WhenHandling<MultiTimeoutSaga.RequestTimeout2>()
                .ExpectSend<MultiTimeoutSaga.SentFromTimeout1>()
                .ExpectSend<MultiTimeoutSaga.SentFromTimeout2>()
                .WhenSagaTimesOut();
        }

        [Test]
        public void ShouldDiscardTimeoutsAfterFirstInvocation()
        {
            Test.Saga<TimeoutSaga>()
                .WhenHandling<TimeoutSaga.StartMessage>()
                .ExpectSend<TimeoutSaga.SendMessage1>()
                .WhenSagaTimesOut()
                .ExpectNotSend<TimeoutSaga.SendMessage1>()
                .WhenSagaTimesOut();
        }
    }

    public class TimeoutSaga : NServiceBus.Saga<TimeoutSaga.TimeoutData>,
        IHandleMessages<TimeoutSaga.StartMessage>,
        IHandleTimeouts<TimeoutSaga.TimeoutMessage1>,
        IHandleTimeouts<TimeoutSaga.TimeoutMessage2>,
        IHandleTimeouts<TimeoutSaga.DateTimeTimeOutMessage1>,
        IHandleTimeouts<TimeoutSaga.DateTimeTimeOutMessage2>
    {
        public async Task Handle(StartMessage message, IMessageHandlerContext context)
        {
            await RequestTimeout(context, TimeSpan.FromDays(7), new TimeoutMessage1());
            await RequestTimeout(context, TimeSpan.FromDays(14), new TimeoutMessage2());
            await RequestTimeout(context, new DateTimeOffset(2010, 1, 7, 0, 0, 0, TimeSpan.Zero), new DateTimeTimeOutMessage1());
            await RequestTimeout(context, new DateTimeOffset(2010, 1, 14, 0, 0, 0, TimeSpan.Zero), new DateTimeTimeOutMessage2());
        }

        public Task Timeout(TimeoutMessage1 state, IMessageHandlerContext context)
        {
            return context.Send(new SendMessage1());
        }

        public Task Timeout(TimeoutMessage2 state, IMessageHandlerContext context)
        {
            return context.Send(new SendMessage2());
        }

        public Task Timeout(DateTimeTimeOutMessage1 state, IMessageHandlerContext context)
        {
            return context.Send(new SendMessageDateTime1());
        }

        public Task Timeout(DateTimeTimeOutMessage2 state, IMessageHandlerContext context)
        {
            return context.Send(new SendMessageDateTime2());
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

        public class SendMessage1 { }

        public class SendMessage2 { }

        public class SendMessageDateTime1 { }

        public class SendMessageDateTime2 { }

        public class TimeoutMessage1 { }

        public class TimeoutMessage2 { }

        public class DateTimeTimeOutMessage1 { }

        public class DateTimeTimeOutMessage2 { }

        public class StartMessage { }
    }

    public class MultiTimeoutSaga : NServiceBus.Saga<MultiTimeoutSaga.TimeoutData>,
        IHandleMessages<MultiTimeoutSaga.RequestTimeout1>,
        IHandleMessages<MultiTimeoutSaga.RequestTimeout2>,
        IHandleTimeouts<MultiTimeoutSaga.Timeout1>,
        IHandleTimeouts<MultiTimeoutSaga.Timeout2>
    {
        public Task Handle(RequestTimeout1 message, IMessageHandlerContext context)
        {
            return RequestTimeout<Timeout1>(context, TimeSpan.FromDays(1));
        }

        public Task Handle(RequestTimeout2 message, IMessageHandlerContext context)
        {
            return RequestTimeout<Timeout2>(context, TimeSpan.FromDays(1));
        }

        public Task Timeout(Timeout1 state, IMessageHandlerContext context)
        {
            return context.Send(new SentFromTimeout1());
        }

        public Task Timeout(Timeout2 state, IMessageHandlerContext context)
        {
            return context.Send(new SentFromTimeout2());
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<TimeoutData> mapper)
        {
        }

        public class TimeoutData : ContainSagaData
        {
        }

        public class RequestTimeout1 : IMessage { }

        public class RequestTimeout2 : IMessage { }

        public class Timeout1 : IMessage { }

        public class Timeout2 : IMessage { }

        public class SentFromTimeout1 : IMessage { }

        public class SentFromTimeout2 : IMessage { }
    }
}