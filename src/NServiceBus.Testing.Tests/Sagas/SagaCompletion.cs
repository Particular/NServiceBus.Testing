namespace NServiceBus.Testing.Tests.Sagas
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class SagaCompletion
    {
        [Test]
        public async Task MarkStartedSagaAsComplete()
        {
            var saga = new TestableSaga<TestSaga, TestSagaData>();

            var result = await saga.Handle(new StartSagaMessage() { CorrelationId = Guid.NewGuid() });

            Assert.IsTrue(result.Completed);
        }

        class TestSaga : NServiceBus.Saga<TestSagaData>, IAmStartedByMessages<StartSagaMessage>
        {
            protected override void ConfigureHowToFindSaga(SagaPropertyMapper<TestSagaData> mapper) => mapper
                .MapSaga(d => d.CorrelationId)
                .ToMessage<StartSagaMessage>(m => m.CorrelationId);

            public Task Handle(StartSagaMessage message, IMessageHandlerContext context)
            {
                MarkAsComplete();
                return Task.CompletedTask;
            }
        }

        class TestSagaData : ContainSagaData
        {
            public Guid CorrelationId { get; set; }
        }

        class StartSagaMessage : IMessage
        {
            public Guid CorrelationId { get; set; }
        }
    }
}