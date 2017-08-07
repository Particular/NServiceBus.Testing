namespace NServiceBus.Testing.Tests.Saga
{
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class ExpectReplyToOriginatorTests
    {
        [Test]
        public void ExpectReplyToOriginatorShouldSucceedWhenRepliedToOriginator()
        {
            Test.Saga<ReplyingToOriginator>()
                .ExpectReplyToOriginator<ReplyMessage>()
                .WhenHandling<StartSagaAndReplyToOriginator>();
        }

        [Test]
        public void ExpectReplyToOriginatorShouldFailWhenNotReplyingToOriginator()
        {
            Assert.Throws<ExpectationException>(() => Test.Saga<ReplyingToOriginator>()
                .ExpectReplyToOriginator<ReplyMessage>()
                .WhenHandling<StartSagaWithoutReplyToOriginator>());
        }

        [Test]
        public void ExpectReplyToOriginatorShouldFailWhenReplyingWithOtherMessageType()
        {
            Assert.Throws<ExpectationException>(() => Test.Saga<ReplyingToOriginator>()
                .ExpectReplyToOriginator<UnexpectedReplyMessage>()
                .WhenHandling<StartSagaAndReplyToOriginator>());
        }

        [Test]
        public void ExpectReplyToOriginatorShouldFailWhenUsingReplyApi()
        {
            Assert.Throws<ExpectationException>(() => Test.Saga<ReplyingToOriginator>()
                .ExpectReplyToOriginator<UnexpectedReplyMessage>()
                .WhenHandling<StartSagaAndReply>());
        }

        public class ReplyingToOriginator : NServiceBus.Saga<MySagaData>,
            IAmStartedByMessages<StartSagaAndReplyToOriginator>,
            IAmStartedByMessages<StartSagaWithoutReplyToOriginator>,
            IAmStartedByMessages<StartSagaAndReply>
        {
            protected override void ConfigureHowToFindSaga(SagaPropertyMapper<MySagaData> mapper)
            {
            }

            public Task Handle(StartSagaAndReplyToOriginator message, IMessageHandlerContext context)
            {
                return ReplyToOriginator(context, new ReplyMessage());
            }

            public Task Handle(StartSagaWithoutReplyToOriginator message, IMessageHandlerContext context)
            {
                return Task.FromResult(0);
            }

            public Task Handle(StartSagaAndReply message, IMessageHandlerContext context)
            {
                var options = new ReplyOptions();
                options.SetDestination("AnotherDestination");

                return context.Reply(new UnexpectedReplyMessage(), options);
            }
        }

        public class MySagaData : ContainSagaData
        {
        }

        public class StartSagaAndReplyToOriginator : ICommand
        {
        }

        public class StartSagaWithoutReplyToOriginator : ICommand
        {
        }

        public class StartSagaAndReply : ICommand
        {
        }

        public class ReplyMessage : IMessage
        {
        }

        public class UnexpectedReplyMessage : IMessage
        {
        }
    }
}