namespace NServiceBus.Testing.Tests.Sagas
{
    using System.Linq;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class SendToSelf
    {
        [Test]
        public async Task ShouldEnqueueMessageForProcessing()
        {
            var testableSaga = new TestableSaga<MySaga, MyData>();

            var startResult = await testableSaga.Handle(new StartMsg { CorrId = "abc" });

            Assert.That(startResult.Context.SentMessages.Count, Is.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(testableSaga.QueueLength, Is.EqualTo(1));
                Assert.That(testableSaga.QueuePeek().Type, Is.EqualTo(typeof(SendToSelfCmd)));
            });

            var continueResult = await testableSaga.HandleQueuedMessage();
            var doneEvt = continueResult.FindPublishedMessage<DoneEvent>();

            Assert.That(doneEvt.CorrId, Is.EqualTo("abc"));
        }

        public class MySaga : Saga<MyData>,
            IAmStartedByMessages<StartMsg>,
            IHandleMessages<SendToSelfCmd>
        {
            protected override void ConfigureHowToFindSaga(SagaPropertyMapper<MyData> mapper)
            {
                mapper.MapSaga(saga => saga.CorrId)
                    .ToMessage<StartMsg>(msg => msg.CorrId);
            }

            public Task Handle(StartMsg message, IMessageHandlerContext context)
            {
                return context.SendLocal(new SendToSelfCmd());
            }
            public Task Handle(SendToSelfCmd message, IMessageHandlerContext context)
            {
                return context.Publish(new DoneEvent { CorrId = Data.CorrId });
            }

        }

        public class MyData : ContainSagaData
        {
            public string CorrId { get; set; }
        }

        public class StartMsg : IEvent
        {
            public string CorrId { get; set; }
        }

        public class SendToSelfCmd : ICommand
        {
            public string CorrId { get; set; }
        }

        public class DoneEvent : IEvent
        {
            public string CorrId { get; set; }
        }
    }
}
