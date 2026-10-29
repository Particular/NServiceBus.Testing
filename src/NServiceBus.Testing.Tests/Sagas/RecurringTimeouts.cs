namespace NServiceBus.Testing.Tests.Sagas;

using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

[TestFixture]
public class RecurringTimeouts
{
    [Test]
    public async Task RecurringTimeoutShouldFireForEveryIntervalWithinAdvanceTime()
    {
        var startTime = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc);
        var testableSaga = new TestableSaga<MySaga, MyData>(initialCurrentTime: startTime);

        await testableSaga.Handle(new Start { CorrId = "abc", Interval = TimeSpan.FromMinutes(10) });

        var timeoutResults = await testableSaga.AdvanceTime(TimeSpan.FromHours(1));
        // The next reminder is due at 70 minutes because each one is scheduled from its own due time
        var noResults = await testableSaga.AdvanceTime(TimeSpan.FromMinutes(9));
        var nextResults = await testableSaga.AdvanceTime(TimeSpan.FromMinutes(1));

        Assert.Multiple(() =>
        {
            Assert.That(timeoutResults.Select(r => r.SagaDataSnapshot.RemindersReceived), Is.EqualTo(new[] { 1, 2, 3, 4, 5, 6 }));
            Assert.That(noResults, Has.Length.Zero);
            Assert.That(nextResults.Single().SagaDataSnapshot.RemindersReceived, Is.EqualTo(7));
        });
    }

    [Test]
    public async Task TimeoutRequestedForCurrentTimeOverAndOverShouldThrow()
    {
        var testableSaga = new TestableSaga<MySaga, MyData>();

        await testableSaga.Handle(new Start { CorrId = "abc", Interval = TimeSpan.Zero });

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await testableSaga.AdvanceTime(TimeSpan.FromMinutes(1)));

        Assert.That(exception.Message, Contains.Substring("without time moving forward"));
    }

    public class MySaga : Saga<MyData>,
        IAmStartedByMessages<Start>,
        IHandleTimeouts<Reminder>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<MyData> mapper) =>
            mapper.MapSaga(saga => saga.CorrId)
                .ToMessage<Start>(msg => msg.CorrId);

        public Task Handle(Start message, IMessageHandlerContext context)
        {
            Data.Interval = message.Interval;
            return RequestTimeout<Reminder>(context, Data.Interval);
        }

        public Task Timeout(Reminder state, IMessageHandlerContext context)
        {
            Data.RemindersReceived++;
            return RequestTimeout<Reminder>(context, Data.Interval);
        }
    }

    public class MyData : ContainSagaData
    {
        public string CorrId { get; set; }
        public TimeSpan Interval { get; set; }
        public int RemindersReceived { get; set; }
    }

    public class Start : ICommand
    {
        public string CorrId { get; set; }
        public TimeSpan Interval { get; set; }
    }

    public class Reminder { }
}
