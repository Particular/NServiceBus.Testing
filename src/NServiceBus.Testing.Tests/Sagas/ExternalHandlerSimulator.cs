namespace NServiceBus.Testing.Tests.Sagas
{
    using System.Linq;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class ExternalHandlerSimulator
    {
        [Test]
        public async Task ExternalHandlerShouldSupplyReplyMessage()
        {
            var testableSaga = new TestableSaga<MySaga, MyData>();

            testableSaga.SimulateReply<RunStep1Command, Step1Response>(cmd => new Step1Response());

            var startResult = await testableSaga.Handle(new StartMsg { CorrId = "abc" });

            Assert.That(startResult.Context.SentMessages.Count, Is.EqualTo(1));
            Assert.That(testableSaga.QueueLength, Is.EqualTo(1));
            Assert.That(testableSaga.QueuePeek().Type, Is.EqualTo(typeof(Step1Response)));

            var continueResult = await testableSaga.HandleQueuedMessage();
            var doneEvt = continueResult.FirstPublishedMessageOrDefault<DoneEvent>();

            Assert.That(doneEvt.CorrId, Is.EqualTo("abc"));
        }

        public class MySaga : NServiceBus.Saga<MyData>,
            IAmStartedByMessages<StartMsg>,
            IHandleMessages<Step1Response>
        {
            protected override void ConfigureHowToFindSaga(SagaPropertyMapper<MyData> mapper)
            {
                mapper.MapSaga(saga => saga.CorrId)
                    .ToMessage<StartMsg>(msg => msg.CorrId);
            }

            public Task Handle(StartMsg message, IMessageHandlerContext context)
            {
                return context.Send(new RunStep1Command());
            }
            public Task Handle(Step1Response message, IMessageHandlerContext context)
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

        public class RunStep1Command : ICommand
        {
        }

        public class Step1Response : IMessage
        {
        }

        public class DoneEvent : IEvent
        {
            public string CorrId { get; set; }
        }
    }
}
