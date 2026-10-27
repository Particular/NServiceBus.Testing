namespace NServiceBus.Testing.Tests.Sagas;

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Time.Testing;
using NUnit.Framework;

[TestFixture]
public class InjectedTimeProvider
{
    [Test]
    public async Task SagaShouldReceiveTimeProviderStartingAtUtcNow()
    {
        var before = DateTimeOffset.UtcNow;
        var testableSaga = new TestableSaga<MySaga, MyData>(timeProvider => new MySaga(timeProvider));
        var after = DateTimeOffset.UtcNow;

        var startResult = await testableSaga.Handle(new Start { CorrId = "abc" });

        Assert.Multiple(() =>
        {
            Assert.That(startResult.SagaDataSnapshot.StartHandledAt, Is.InRange(before, after));
            Assert.That(testableSaga.CurrentTime, Is.EqualTo(startResult.SagaDataSnapshot.StartHandledAt.UtcDateTime));
        });
    }

    [Test]
    public async Task SagaShouldReceiveProvidedTimeProvider()
    {
        var startTime = new DateTimeOffset(2026, 1, 1, 8, 0, 0, TimeSpan.Zero);
        var timeProvider = new FakeTimeProvider(startTime);
        var testableSaga = new TestableSaga<MySaga, MyData>(_ => new MySaga(timeProvider), timeProvider);

        var startResult = await testableSaga.Handle(new Start { CorrId = "abc" });

        Assert.Multiple(() =>
        {
            Assert.That(testableSaga.TimeProvider, Is.SameAs(timeProvider));
            Assert.That(testableSaga.CurrentTime, Is.EqualTo(startTime.UtcDateTime));
            Assert.That(startResult.SagaDataSnapshot.StartHandledAt, Is.EqualTo(startTime));
        });
    }

    [Test]
    public async Task TimeoutHandlerShouldReadTimeoutDueTimeFromTimeProvider()
    {
        var startTime = new DateTimeOffset(2026, 1, 1, 8, 0, 0, TimeSpan.Zero);
        var testableSaga = new TestableSaga<MySaga, MyData>(timeProvider => new MySaga(timeProvider), new FakeTimeProvider(startTime));

        await testableSaga.Handle(new Start { CorrId = "abc" });
        var timeoutResults = await testableSaga.AdvanceTime(TimeSpan.FromHours(1));

        Assert.Multiple(() =>
        {
            Assert.That(timeoutResults[0].SagaDataSnapshot.EarlyTimeoutHandledAt, Is.EqualTo(startTime.AddMinutes(10)));
            Assert.That(timeoutResults[1].SagaDataSnapshot.LateTimeoutHandledAt, Is.EqualTo(startTime.AddMinutes(30)));
            Assert.That(testableSaga.TimeProvider.GetUtcNow(), Is.EqualTo(startTime.AddHours(1)));
        });
    }

    [Test]
    public async Task TimeoutShouldBeScheduledRelativeToAdvancedTimeProvider()
    {
        var startTime = new DateTimeOffset(2026, 1, 1, 8, 0, 0, TimeSpan.Zero);
        var timeProvider = new FakeTimeProvider(startTime);
        var testableSaga = new TestableSaga<MySaga, MyData>(_ => new MySaga(timeProvider), timeProvider);

        timeProvider.Advance(TimeSpan.FromHours(1));
        await testableSaga.Handle(new Start { CorrId = "abc" });

        var noResults = await testableSaga.AdvanceTime(TimeSpan.FromMinutes(9));
        Assert.That(noResults, Has.Length.Zero);

        var timeoutResults = await testableSaga.AdvanceTime(TimeSpan.FromMinutes(1));
        Assert.That(timeoutResults.Single().SagaDataSnapshot.EarlyTimeoutHandledAt, Is.EqualTo(startTime.AddMinutes(70)));
    }

    [Test]
    public async Task TimeoutsMadeDueByTimeProviderShouldBeHandledOnNextAdvanceTime()
    {
        var startTime = new DateTimeOffset(2026, 1, 1, 8, 0, 0, TimeSpan.Zero);
        var timeProvider = new FakeTimeProvider(startTime);
        var testableSaga = new TestableSaga<MySaga, MyData>(_ => new MySaga(timeProvider), timeProvider);

        await testableSaga.Handle(new Start { CorrId = "abc" });
        timeProvider.Advance(TimeSpan.FromHours(1));

        var timeoutResults = await testableSaga.AdvanceTime(TimeSpan.Zero);

        // Overdue timeouts run at the current time because the time provider cannot go backwards
        Assert.Multiple(() =>
        {
            Assert.That(timeoutResults.Select(r => r.HandledMessage.Type), Is.EqualTo(new[] { typeof(EarlyTimeout), typeof(LateTimeout) }));
            Assert.That(timeoutResults[0].SagaDataSnapshot.EarlyTimeoutHandledAt, Is.EqualTo(startTime.AddHours(1)));
            Assert.That(timeoutResults[1].SagaDataSnapshot.LateTimeoutHandledAt, Is.EqualTo(startTime.AddHours(1)));
        });
    }

    public class MySaga(TimeProvider timeProvider) : Saga<MyData>,
        IAmStartedByMessages<Start>,
        IHandleTimeouts<EarlyTimeout>,
        IHandleTimeouts<LateTimeout>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<MyData> mapper) =>
            mapper.MapSaga(saga => saga.CorrId)
                .ToMessage<Start>(msg => msg.CorrId);

        public async Task Handle(Start message, IMessageHandlerContext context)
        {
            Data.StartHandledAt = timeProvider.GetUtcNow();

            await RequestTimeout<EarlyTimeout>(context, TimeSpan.FromMinutes(10));
            await RequestTimeout<LateTimeout>(context, TimeSpan.FromMinutes(30));
        }

        public Task Timeout(EarlyTimeout state, IMessageHandlerContext context)
        {
            Data.EarlyTimeoutHandledAt = timeProvider.GetUtcNow();
            return Task.CompletedTask;
        }

        public Task Timeout(LateTimeout state, IMessageHandlerContext context)
        {
            Data.LateTimeoutHandledAt = timeProvider.GetUtcNow();
            return Task.CompletedTask;
        }
    }

    public class MyData : ContainSagaData
    {
        public string CorrId { get; set; }
        public DateTimeOffset StartHandledAt { get; set; }
        public DateTimeOffset EarlyTimeoutHandledAt { get; set; }
        public DateTimeOffset LateTimeoutHandledAt { get; set; }
    }

    public class Start : ICommand
    {
        public string CorrId { get; set; }
    }

    public class EarlyTimeout { }

    public class LateTimeout { }
}
