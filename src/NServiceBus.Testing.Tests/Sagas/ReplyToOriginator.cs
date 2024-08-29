namespace NServiceBus.Testing.Tests.Sagas
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class ReplyToOriginator
    {
        [Test]
        public async Task ReplyToOriginatorShouldReplyToInitialOriginator()
        {
            const string originatorAddress = "expectedReplyAddress";

            var saga = new TestableSaga<ReplyingSaga, ReplyingSagaData>();
            // the Originator value is populated by the header value, not the context property
            await saga.Handle(new StartSagaMessage() { CorrelationProperty = Guid.NewGuid() }, messageHeaders: new Dictionary<string, string>()
            {
                {Headers.ReplyToAddress, originatorAddress}
            });
            var result = await saga.HandleQueuedMessage();

            var reply = result.Context.RepliedMessages.SingleOrDefault();
            Assert.That(reply, Is.Not.Null);
            Assert.That(reply.Options.GetDestination(), Is.EqualTo(originatorAddress));
            Assert.That(reply.Message<ReplyMessage>().OriginatorAddress, Is.EqualTo(originatorAddress));
        }

        [Test]
        public async Task OriginatorShouldBeSetByDefault()
        {
            // ensure the testing API also works without explicitly defining a replyTo header value
            var saga = new TestableSaga<ReplyingSaga, ReplyingSagaData>();

            await saga.Handle(new StartSagaMessage() { CorrelationProperty = Guid.NewGuid() });
            var result = await saga.HandleQueuedMessage();

            var reply = result.Context.RepliedMessages.SingleOrDefault();
            Assert.That(reply, Is.Not.Null);
            string replyAddress = reply.Options.GetDestination();
            Assert.That(replyAddress, Is.Not.Null);
            Assert.That(reply.Message<ReplyMessage>().OriginatorAddress, Is.EqualTo(replyAddress));
        }

        class ReplyingSaga : Saga<ReplyingSagaData>, IAmStartedByMessages<StartSagaMessage>, IHandleMessages<SendReplyMessage>
        {
            protected override void ConfigureHowToFindSaga(SagaPropertyMapper<ReplyingSagaData> mapper) => mapper
                .ConfigureMapping<StartSagaMessage>(m => m.CorrelationProperty)
                .ToSaga(s => s.CorrelationProperty);

            public Task Handle(StartSagaMessage message, IMessageHandlerContext context)
            {
                return context.SendLocal(new SendReplyMessage());
            }

            public Task Handle(SendReplyMessage message, IMessageHandlerContext context)
            {
                return ReplyToOriginator(context, new ReplyMessage { OriginatorAddress = Data.Originator });
            }
        }

        class ReplyingSagaData : ContainSagaData
        {
            public Guid CorrelationProperty { get; set; }
        }

        class StartSagaMessage : IMessage
        {
            public Guid CorrelationProperty { get; set; }
        }

        class SendReplyMessage : ICommand
        {
            public Guid CorrelationProperty { get; set; }
        }

        class ReplyMessage : IMessage
        {
            public string OriginatorAddress { get; set; }
        }
    }
}