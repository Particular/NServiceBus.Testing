namespace NServiceBus.Testing.Tests.Sagas;

using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

[TestFixture]
public class AbsoluteTimeouts
{
    [Test]
    public async Task TimeoutShouldBeDueAtRequestedTime()
    {
        var startTime = new DateTimeOffset(2026, 1, 1, 8, 0, 0, TimeSpan.Zero);
        var testableSaga = new TestableSaga<MySaga, MyData>(initialCurrentTime: startTime.UtcDateTime);

        await testableSaga.Handle(new StartWithTimeout { CorrId = "abc", TimeoutAt = startTime.AddMinutes(20) });

        var noResults = await testableSaga.AdvanceTime(TimeSpan.FromMinutes(19));
        Assert.That(noResults, Has.Length.EqualTo(0));

        var timeoutResults = await testableSaga.AdvanceTime(TimeSpan.FromMinutes(1));
        Assert.That(timeoutResults.Single().HandledMessage.Type, Is.EqualTo(typeof(MyTimeout)));
    }

    [Test]
    public async Task DelayedMessageShouldBeDueAtRequestedTime()
    {
        var startTime = new DateTimeOffset(2026, 1, 1, 8, 0, 0, TimeSpan.Zero);
        var testableSaga = new TestableSaga<MySaga, MyData>(initialCurrentTime: startTime.UtcDateTime);

        await testableSaga.Handle(new StartWithDelayedCmd { CorrId = "abc", DeliverAt = startTime.AddMinutes(20) });

        var noResults = await testableSaga.AdvanceTime(TimeSpan.FromMinutes(19));
        Assert.That(noResults, Has.Length.EqualTo(0));

        var delayedCmdResults = await testableSaga.AdvanceTime(TimeSpan.FromMinutes(1));
        Assert.That(delayedCmdResults.Single().SagaDataSnapshot.DelayedCommandReceived, Is.True);
    }

    [Test]
    public async Task TimeoutInThePastShouldBeHandledAtItsPastDueTime()
    {
        var startTime = new DateTimeOffset(2026, 1, 1, 8, 0, 0, TimeSpan.Zero);
        var testableSaga = new TestableSaga<MySaga, MyData>(initialCurrentTime: startTime.UtcDateTime);

        await testableSaga.AdvanceTime(TimeSpan.FromHours(1));
        await testableSaga.Handle(new StartWithTimeout { CorrId = "abc", TimeoutAt = startTime.AddMinutes(30) });

        var timeoutResults = await testableSaga.AdvanceTime(TimeSpan.FromMinutes(1));
        // The follow-up is due at 45 minutes because the timeout ran at its past due time of 30 minutes
        var followUpResults = await testableSaga.AdvanceTime(TimeSpan.Zero);

        Assert.Multiple(() =>
        {
            Assert.That(timeoutResults.Single().HandledMessage.Type, Is.EqualTo(typeof(MyTimeout)));
            Assert.That(followUpResults.Single().HandledMessage.Type, Is.EqualTo(typeof(FollowUpTimeout)));
            Assert.That(testableSaga.CurrentTime, Is.EqualTo(startTime.AddMinutes(61).UtcDateTime));
        });
    }

    public class MySaga : Saga<MyData>,
        IAmStartedByMessages<StartWithTimeout>,
        IAmStartedByMessages<StartWithDelayedCmd>,
        IHandleMessages<DelayedCmd>,
        IHandleTimeouts<MyTimeout>,
        IHandleTimeouts<FollowUpTimeout>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<MyData> mapper) =>
            mapper.MapSaga(saga => saga.CorrId)
                .ToMessage<StartWithTimeout>(msg => msg.CorrId)
                .ToMessage<StartWithDelayedCmd>(msg => msg.CorrId)
                .ToMessage<DelayedCmd>(msg => msg.CorrId);

        public Task Handle(StartWithTimeout message, IMessageHandlerContext context) => RequestTimeout<MyTimeout>(context, message.TimeoutAt);

        public Task Handle(StartWithDelayedCmd message, IMessageHandlerContext context)
        {
            var opts = new SendOptions();
            opts.DoNotDeliverBefore(message.DeliverAt);
            return context.Send(new DelayedCmd { CorrId = message.CorrId }, opts);
        }

        public Task Handle(DelayedCmd message, IMessageHandlerContext context)
        {
            Data.DelayedCommandReceived = true;
            return Task.CompletedTask;
        }

        public Task Timeout(MyTimeout state, IMessageHandlerContext context) => RequestTimeout<FollowUpTimeout>(context, TimeSpan.FromMinutes(15));

        public Task Timeout(FollowUpTimeout state, IMessageHandlerContext context) => Task.CompletedTask;
    }

    public class MyData : ContainSagaData
    {
        public string CorrId { get; set; }
        public bool DelayedCommandReceived { get; set; }
    }

    public class StartWithTimeout : ICommand
    {
        public string CorrId { get; set; }
        public DateTimeOffset TimeoutAt { get; set; }
    }

    public class StartWithDelayedCmd : ICommand
    {
        public string CorrId { get; set; }
        public DateTimeOffset DeliverAt { get; set; }
    }

    public class DelayedCmd : ICommand
    {
        public string CorrId { get; set; }
    }

    public class MyTimeout { }

    public class FollowUpTimeout { }
}
