namespace NServiceBus.Testing.Tests.Saga
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class ExpectTimeoutTests
    {
        [Test]
        public void TimeoutInThePast()
        {
            var expected = DateTime.UtcNow.AddDays(-3);
            var message = new TheMessage
            {
                TimeoutAt = expected
            };

            Test.Saga<TimeoutSaga>()
                .ExpectTimeoutToBeSetAt<TheTimeout>((m, at) => at == expected)
                .When((s, c) => s.Handle(message, c));
        }

        [Test]
        public void TimeoutInThePastWithSendOnTimeout()
        {
            var message = new TheMessage
            {
                TimeoutAt = DateTime.UtcNow.AddDays(-3)
            };

            Test.Saga<TimeoutSaga>()
                .ExpectTimeoutToBeSetAt<TheTimeout>((m, at) => true)
                .When((s, c) => s.Handle(message, c))
                .ExpectSend<TheMessageSentAtTimeout>()
                .WhenHandlingTimeout<TheTimeout>();
        }

        [Test]
        public void TimeoutInTheFuture()
        {
            var expected = DateTime.UtcNow.AddDays(3);
            var message = new TheMessage
            {
                TimeoutAt = expected
            };

            Test.Saga<TimeoutSaga>()
                .ExpectTimeoutToBeSetAt<TheTimeout>((m, at) => at == expected)
                .When((s, c) => s.Handle(message, c));
        }

        [Test]
        public void Should_assert_30_style_timeouts_being_set()
        {
            Test.Saga<MultipleTimeoutsSaga>()
                .ExpectTimeoutToBeSetIn<MyTimeout>()
                .When((s, c) => s.Handle(new StartMessage(), c));
        }

        [Test]
        public void Should_assert_30_style_timeouts_being_set_together_with_other_timeouts()
        {
            Test.Saga<MultipleTimeoutsSaga>()
                .ExpectTimeoutToBeSetIn<MyTimeout>()
                .When((s, c) => s.Handle(new StartMessage(), c));
        }

        [Test]
        public void Should_assert_30_style_timeouts_being_set_with_the_correct_timeSpan()
        {
            Test.Saga<MultipleTimeoutsSaga>()
                .ExpectTimeoutToBeSetIn<MyTimeout>((state, expiresIn) => expiresIn == TimeSpan.FromDays(1))
                .When((s, c) => s.Handle(new StartMessage(), c));
        }

        [Test]
        public void Should_assert_30_style_timeouts_being_set_with_the_correct_state()
        {
            Test.Saga<MultipleTimeoutsSaga>()
                .ExpectTimeoutToBeSetIn<MyTimeout>((state, expiresIn) => state.SomeProperty == "Test")
                .When((s, c) => s.Handle(new StartMessage(), c));
        }

        public class TimeoutSaga : NServiceBus.Saga<MyTimeoutData>,
            IAmStartedByMessages<TheMessage>,
            IHandleTimeouts<TheTimeout>
        {
            public Task Handle(TheMessage message, IMessageHandlerContext context)
            {
                return RequestTimeout<TheTimeout>(context, message.TimeoutAt);
            }

            public async Task Timeout(TheTimeout state, IMessageHandlerContext context)
            {
                await context.Send(new TheMessageSentAtTimeout());
                MarkAsComplete();
            }

            protected override void ConfigureHowToFindSaga(SagaPropertyMapper<MyTimeoutData> mapper)
            {
            }
        }

        class MultipleTimeoutsSaga : NServiceBus.Saga<TimeoutSagaData>,
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

            public Task Timeout(MyOtherTimeout state, IMessageHandlerContext context)
            {
                return Task.FromResult(0);
            }

            public Task Timeout(MyTimeout state, IMessageHandlerContext context)
            {
                return context.Send(new SomeMessage());
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

        public class MyTimeoutData : IContainSagaData
        {
            public Guid Id { get; set; }
            public string Originator { get; set; }
            public string OriginalMessageId { get; set; }
        }

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