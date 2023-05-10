namespace NServiceBus.Testing.Tests.Sagas
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class SagaCompletionWithTimeouts
    {
        [Test]
        public async Task TimeoutShouldBeSwallowedAfterSagaCompletion()
        {
            var saga = new TestableSaga<MyCustomSaga, MyCustomSagaData>();

            await saga.Handle(new MsgHappensTwice { CorrId = "12345" });
            await saga.Handle(new MsgHappensTwice { CorrId = "12345" });

            var result = await saga.AdvanceTime(TimeSpan.FromHours(2));

            Assert.That(result.Length, Is.EqualTo(1));
            Assert.That(result[0].Completed, Is.True);
        }

        public class MyCustomSaga : NServiceBus.Saga<MyCustomSagaData>,
            IAmStartedByMessages<MsgHappensTwice>,
            IHandleTimeouts<TimeoutHappensTwice>
        {
            protected override void ConfigureHowToFindSaga(SagaPropertyMapper<MyCustomSagaData> mapper)
            {
                mapper.MapSaga(saga => saga.CorrId)
                    .ToMessage<MsgHappensTwice>(message => message.CorrId);
            }

            public Task Handle(MsgHappensTwice message, IMessageHandlerContext context)
            {
                return RequestTimeout<TimeoutHappensTwice>(context, TimeSpan.FromHours(1));
            }

            public Task Timeout(TimeoutHappensTwice state, IMessageHandlerContext context)
            {
                MarkAsComplete();
                return Task.CompletedTask;
            }
        }

        public class MsgHappensTwice : ICommand
        {
            public string CorrId { get; set; }
        }

        public class TimeoutHappensTwice { }

        public class MyCustomSagaData : ContainSagaData
        {
            public string CorrId { get; set; }
        }
    }
}
