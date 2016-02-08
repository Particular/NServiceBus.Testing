namespace NServiceBus.Testing.Tests
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class Timeouts
    {
        [Test]
        public void Should_assert_30_style_timeouts_being_set()
        {
            Test.Saga<TimeoutSaga>()
                .ExpectTimeoutToBeSetIn<MyTimeout>()
                .When((s, c) => s.Handle(new StartMessage(), c));
        }

        [Test]
        public void Should_assert_30_style_timeouts_being_set_together_with_other_timeouts()
        {
            Test.Saga<TimeoutSaga>()
                .ExpectTimeoutToBeSetIn<MyTimeout>()
                .When((s, c) => s.Handle(new StartMessage(), c));
        }

        [Test]
        public void Should_assert_30_style_timeouts_being_set_with_the_correct_timeSpan()
        {
            Test.Saga<TimeoutSaga>()
                .ExpectTimeoutToBeSetIn<MyTimeout>((state, expiresIn) => expiresIn == TimeSpan.FromDays(1))
                .When((s, c) => s.Handle(new StartMessage(), c));
        }

        [Test]
        public void Should_assert_30_style_timeouts_being_set_with_the_correct_state()
        {
            Test.Saga<TimeoutSaga>()
                .ExpectTimeoutToBeSetIn<MyTimeout>((state, expiresIn) => state.SomeProperty == "Test")
                .When((s, c) => s.Handle(new StartMessage(), c));
        }
    }

    class TimeoutSaga : NServiceBus.Saga<TimeoutSagaData>,
                                 IHandleTimeouts<MyTimeout>,
                                 IHandleTimeouts<MyOtherTimeout>,
                                 IAmStartedByMessages<StartMessage>
    {

        public async Task Handle(StartMessage message, IMessageHandlerContext context)
        {
            await RequestTimeout(context, TimeSpan.FromDays(1), new MyTimeout
            {
                SomeProperty = "Test"
            });

            await RequestTimeout<MyOtherTimeout>(context, TimeSpan.FromDays(1));
        }

        public Task Timeout(MyTimeout state, IMessageHandlerContext context)
        {
            return context.Send(new SomeMessage());
        }

        public Task Timeout(MyOtherTimeout state, IMessageHandlerContext context)
        {
            return Task.FromResult(0);
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<TimeoutSagaData> mapper)
        {
        }
    }

    class StartMessage
    {
    }

    class SomeMessage : IMessage
    {
    }

    class MyTimeout
    {
        public string SomeProperty { get; set; }
    }

    class MyOtherTimeout
    {
    }

    class TimeoutSagaData : IContainSagaData
    {
        public Guid Id { get; set; }
        public string Originator { get; set; }
        public string OriginalMessageId { get; set; }
    }
}
