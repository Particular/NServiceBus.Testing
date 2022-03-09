namespace NServiceBus.Testing.Tests.Sagas
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class SagaWithReplyMessage
    {
        [Test]
        public async Task TestReplyMessageApi()
        {
            var testableSaga = new TestableSaga<MySaga, MyData>();
            var processId = Guid.NewGuid().ToString().Substring(0, 8);

            var startResult = await testableSaga.Handle(new StartMsg { ProcessId = processId });

            var step1Cmd = startResult.FindSentMessage<Step1Cmd>();
            Assert.That(step1Cmd, Is.Not.Null);
            Assert.That(startResult.SagaDataSnapshot.ReplyReceived, Is.False);

            var sagaId = startResult.SagaId;

            var endResult = await testableSaga.HandleReply(sagaId, new Step1Reply());
            Assert.That(endResult.SagaDataSnapshot.ReplyReceived, Is.True);
        }

        public class MySaga : NServiceBus.Saga<MyData>,
            IAmStartedByMessages<StartMsg>,
            IHandleMessages<Step1Reply>
        {
            protected override void ConfigureHowToFindSaga(SagaPropertyMapper<MyData> mapper)
            {
                mapper.MapSaga(saga => saga.ProcessId)
                    .ToMessage<StartMsg>(msg => msg.ProcessId);
            }

            public async Task Handle(StartMsg message, IMessageHandlerContext context)
            {
                await context.Send(new Step1Cmd());
            }
            public Task Handle(Step1Reply message, IMessageHandlerContext context)
            {
                Data.ReplyReceived = true;
                return Task.CompletedTask;
            }
        }

        public class MyData : ContainSagaData
        {
            public string ProcessId { get; set; }
            public bool ReplyReceived { get; set; }
        }

        public class StartMsg : IEvent
        {
            public string ProcessId { get; set; }
        }

        public class Step1Cmd : ICommand { }
        public class Step1Reply : IMessage { }
    }
}
