namespace NServiceBus.Testing.Tests.Sagas;

using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

[TestFixture]
public class CurrentTime
{
    [Test]
    public void CurrentTimeShouldDefaultToUtcNow()
    {
        var before = DateTime.UtcNow;
        var testableSaga = new TestableSaga<MySaga, MyData>();
        var after = DateTime.UtcNow;

        Assert.Multiple(() =>
        {
            Assert.That(testableSaga.CurrentTime, Is.InRange(before, after));
            Assert.That(testableSaga.CurrentTime.Kind, Is.EqualTo(DateTimeKind.Utc));
        });
    }

    [Test]
    public void CurrentTimeShouldStartAtInitialCurrentTime()
    {
        var startTime = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc);
        var testableSaga = new TestableSaga<MySaga, MyData>(initialCurrentTime: startTime);

        Assert.That(testableSaga.CurrentTime, Is.EqualTo(startTime));
    }

    [Test]
    public async Task InitialCurrentTimeWithUnspecifiedKindShouldNotBeShifted()
    {
        // Only detects a local time conversion on machines that are not set to UTC
        var startTime = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Unspecified);
        var testableSaga = new TestableSaga<MySaga, MyData>(initialCurrentTime: startTime);

        await testableSaga.Handle(new Start { CorrId = "abc" });

        Assert.That(testableSaga.CurrentTime, Is.EqualTo(startTime));

        var noResults = await testableSaga.AdvanceTime(TimeSpan.FromMinutes(14));
        Assert.That(noResults, Has.Length.EqualTo(0));

        var timeoutResults = await testableSaga.AdvanceTime(TimeSpan.FromMinutes(1));
        Assert.That(timeoutResults, Has.Length.EqualTo(1));
    }

    [Test]
    public async Task AdvanceTimeShouldMoveCurrentTimeWhenNothingIsDue()
    {
        var startTime = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc);
        var testableSaga = new TestableSaga<MySaga, MyData>(initialCurrentTime: startTime);

        var noResults = await testableSaga.AdvanceTime(TimeSpan.FromMinutes(10));

        Assert.Multiple(() =>
        {
            Assert.That(noResults, Has.Length.EqualTo(0));
            Assert.That(testableSaga.CurrentTime, Is.EqualTo(startTime.AddMinutes(10)));
        });
    }

    [Test]
    public async Task AdvanceTimeShouldEndAtTargetTimeAfterHandlingTimeouts()
    {
        var startTime = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc);
        var testableSaga = new TestableSaga<MySaga, MyData>(initialCurrentTime: startTime);

        await testableSaga.Handle(new Start { CorrId = "abc" });
        var timeoutResults = await testableSaga.AdvanceTime(TimeSpan.FromHours(1));

        Assert.Multiple(() =>
        {
            Assert.That(timeoutResults, Has.Length.EqualTo(1));
            Assert.That(testableSaga.CurrentTime, Is.EqualTo(startTime.AddHours(1)));
        });
    }

    [Test]
    public async Task TimeoutShouldBeScheduledRelativeToAdvancedTime()
    {
        var startTime = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc);
        var testableSaga = new TestableSaga<MySaga, MyData>(initialCurrentTime: startTime);

        await testableSaga.AdvanceTime(TimeSpan.FromHours(1));
        await testableSaga.Handle(new Start { CorrId = "abc" });

        var noResults = await testableSaga.AdvanceTime(TimeSpan.FromMinutes(14));
        Assert.That(noResults, Has.Length.EqualTo(0));

        var timeoutResults = await testableSaga.AdvanceTime(TimeSpan.FromMinutes(1));
        Assert.That(timeoutResults.Single().SagaDataSnapshot.TimeoutReceived, Is.True);
    }

    public class MySaga : Saga<MyData>,
        IAmStartedByMessages<Start>,
        IHandleTimeouts<MyTimeout>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<MyData> mapper) =>
            mapper.MapSaga(saga => saga.CorrId)
                .ToMessage<Start>(msg => msg.CorrId);

        public Task Handle(Start message, IMessageHandlerContext context) => RequestTimeout<MyTimeout>(context, TimeSpan.FromMinutes(15));

        public Task Timeout(MyTimeout state, IMessageHandlerContext context)
        {
            Data.TimeoutReceived = true;
            return Task.CompletedTask;
        }
    }

    public class MyData : ContainSagaData
    {
        public string CorrId { get; set; }
        public bool TimeoutReceived { get; set; }
    }

    public class Start : ICommand
    {
        public string CorrId { get; set; }
    }

    public class MyTimeout { }
}
