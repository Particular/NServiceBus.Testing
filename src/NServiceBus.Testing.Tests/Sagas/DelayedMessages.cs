namespace NServiceBus.Testing.Tests.Sagas;

using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

[TestFixture]
public class DelayedMessages
{
    [Test]
    public async Task TestDealyedMessages()
    {
        var testableSaga = new TestableSaga<DelaySaga, Data>();

        var corrId = Guid.NewGuid().ToString().Substring(0, 8);

        var startResult = await testableSaga.Handle(new Start { CorrId = corrId });

        Assert.That(startResult.Context.TimeoutMessages.Count, Is.EqualTo(1)); // Just timeout
        Assert.That(startResult.Context.SentMessages.Count, Is.EqualTo(2));    // Both
        Assert.Multiple(() =>
        {
            Assert.That(startResult.SagaDataSnapshot.RegularTimeoutReceived, Is.False);
            Assert.That(startResult.SagaDataSnapshot.DelayedCommandReceived, Is.False);
        });

        var timeout = startResult.FindTimeoutMessage<RegularTimeout>();
        var delayed = startResult.FindSentMessage<DelayedCmd>();

        Assert.Multiple(() =>
        {
            Assert.That(timeout, Is.Not.Null);
            Assert.That(delayed, Is.Not.Null);
        });
        Assert.That(delayed.CorrId, Is.EqualTo(corrId));

        var timeoutResults = await testableSaga.AdvanceTime(TimeSpan.FromMinutes(30));
        var timeoutResult = timeoutResults.Single();
        Assert.Multiple(() =>
        {
            Assert.That(timeoutResult.SagaDataSnapshot.RegularTimeoutReceived, Is.True);
            Assert.That(timeoutResult.SagaDataSnapshot.DelayedCommandReceived, Is.False);
        });

        var delayedCmdResults = await testableSaga.AdvanceTime(TimeSpan.FromMinutes(30));
        var delayedCmdResult = delayedCmdResults.Single();
        Assert.Multiple(() =>
        {
            Assert.That(delayedCmdResult.SagaDataSnapshot.RegularTimeoutReceived, Is.True);
            Assert.That(delayedCmdResult.SagaDataSnapshot.DelayedCommandReceived, Is.True);
        });
    }

    public class DelaySaga : Saga<Data>,
        IAmStartedByMessages<Start>,
        IHandleMessages<DelayedCmd>,
        IHandleTimeouts<RegularTimeout>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<Data> mapper) =>
            mapper.MapSaga(saga => saga.CorrId)
                .ToMessage<Start>(msg => msg.CorrId)
                .ToMessage<DelayedCmd>(msg => msg.CorrId);

        public async Task Handle(Start message, IMessageHandlerContext context)
        {
            // 30m standard timeout
            await RequestTimeout<RegularTimeout>(context, TimeSpan.FromMinutes(30));

            // 1h non-timeout delayed message
            var opts = new SendOptions();
            opts.DelayDeliveryWith(TimeSpan.FromHours(1));
            await context.Send(new DelayedCmd { CorrId = message.CorrId }, opts);
        }
        public Task Handle(DelayedCmd message, IMessageHandlerContext context)
        {
            Data.DelayedCommandReceived = true;
            return Task.CompletedTask;
        }


        public Task Timeout(RegularTimeout state, IMessageHandlerContext context)
        {
            Data.RegularTimeoutReceived = true;
            return Task.CompletedTask;
        }
    }

    public class Data : ContainSagaData
    {
        public string CorrId { get; set; }
        public bool RegularTimeoutReceived { get; set; }
        public bool DelayedCommandReceived { get; set; }
    }

    public class Start : ICommand
    {
        public string CorrId { get; set; }
    }

    public class DelayedCmd : ICommand
    {
        public string CorrId { get; set; }
    }

    public class RegularTimeout { }
}