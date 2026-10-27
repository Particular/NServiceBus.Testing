namespace NServiceBus.Testing.Tests.Sagas;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

[TestFixture]
public class MultipleTimeouts
{
    [Test]
    public async Task DueTimeoutsShouldBeHandledInDueTimeOrder()
    {
        var testableSaga = new TestableSaga<MySaga, MyData>();

        await testableSaga.Handle(new Start { CorrId = "abc" });
        var timeoutResults = await testableSaga.AdvanceTime(TimeSpan.FromHours(1));

        Assert.That(timeoutResults.Select(r => r.HandledMessage.Type), Is.EqualTo(new[] { typeof(EarlyTimeout), typeof(LateTimeout) }));
    }

    [Test]
    public async Task TimeoutsDueAtSameTimeShouldBeHandledInRequestOrder()
    {
        var testableSaga = new TestableSaga<MySaga, MyData>();

        await testableSaga.Handle(new StartSimultaneous { CorrId = "abc", Count = 20, DueTimes = 1 });
        var timeoutResults = await testableSaga.AdvanceTime(TimeSpan.FromHours(1));

        Assert.That(timeoutResults.Select(r => ((NumberedTimeout)r.HandledMessage.Message).Number), Is.EqualTo(Enumerable.Range(0, 20)));
    }

    [Test]
    public async Task ManyTimeoutsDueAtSeveralTimesShouldBeHandledInOneCall()
    {
        var testableSaga = new TestableSaga<MySaga, MyData>();

        await testableSaga.Handle(new StartSimultaneous { CorrId = "abc", Count = 600, DueTimes = 2 });
        var timeoutResults = await testableSaga.AdvanceTime(TimeSpan.FromHours(1));

        Assert.That(timeoutResults, Has.Length.EqualTo(1200));
    }

    [Test]
    public async Task ProvidedContextShouldBeUsedForEachTimeout()
    {
        var testableSaga = new TestableSaga<MySaga, MyData>();
        var providedContexts = new List<TestableMessageHandlerContext>();
        var timeoutsProvidedFor = new List<Type>();

        await testableSaga.Handle(new Start { CorrId = "abc" });
        var timeoutResults = await testableSaga.AdvanceTime(TimeSpan.FromHours(1), provideContext: timeout =>
        {
            var context = new TestableMessageHandlerContext();
            providedContexts.Add(context);
            timeoutsProvidedFor.Add(timeout.Message.GetType());
            return context;
        });

        Assert.Multiple(() =>
        {
            Assert.That(timeoutsProvidedFor, Is.EqualTo(new[] { typeof(EarlyTimeout), typeof(LateTimeout) }));
            Assert.That(timeoutResults.Select(r => r.Context), Is.EqualTo(providedContexts));
        });
    }

    [Test]
    public async Task AdvanceTimeShouldThrowWhenCancelled()
    {
        var testableSaga = new TestableSaga<MySaga, MyData>();
        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        await testableSaga.Handle(new Start { CorrId = "abc" });

        Assert.ThrowsAsync<OperationCanceledException>(async () => await testableSaga.AdvanceTime(TimeSpan.FromHours(1), cancellationToken: cancellationTokenSource.Token));
    }

    [Test]
    public async Task CancelledAdvanceTimeShouldKeepUnhandledTimeouts()
    {
        var testableSaga = new TestableSaga<MySaga, MyData>();
        using var cancellationTokenSource = new CancellationTokenSource();

        await testableSaga.Handle(new Start { CorrId = "abc" });

        Assert.ThrowsAsync<OperationCanceledException>(async () => await testableSaga.AdvanceTime(TimeSpan.FromHours(1), provideContext: timeout =>
        {
            cancellationTokenSource.Cancel();
            return new TestableMessageHandlerContext();
        }, cancellationToken: cancellationTokenSource.Token));

        var timeoutResults = await testableSaga.AdvanceTime(TimeSpan.FromHours(1));

        Assert.That(timeoutResults.Single().HandledMessage.Type, Is.EqualTo(typeof(LateTimeout)));
    }

    [Test]
    public async Task FailedTimeoutShouldNotPreventLaterTimeouts()
    {
        var testableSaga = new TestableSaga<MySaga, MyData>();

        await testableSaga.Handle(new Start { CorrId = "abc", FailEarlyTimeout = true });

        Assert.ThrowsAsync<InvalidOperationException>(async () => await testableSaga.AdvanceTime(TimeSpan.FromHours(1)));

        var timeoutResults = await testableSaga.AdvanceTime(TimeSpan.FromHours(1));

        Assert.That(timeoutResults.Single().HandledMessage.Type, Is.EqualTo(typeof(LateTimeout)));
    }

    public class MySaga : Saga<MyData>,
        IAmStartedByMessages<Start>,
        IAmStartedByMessages<StartSimultaneous>,
        IHandleTimeouts<EarlyTimeout>,
        IHandleTimeouts<LateTimeout>,
        IHandleTimeouts<NumberedTimeout>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<MyData> mapper) =>
            mapper.MapSaga(saga => saga.CorrId)
                .ToMessage<Start>(msg => msg.CorrId)
                .ToMessage<StartSimultaneous>(msg => msg.CorrId);

        public async Task Handle(Start message, IMessageHandlerContext context)
        {
            Data.FailEarlyTimeout = message.FailEarlyTimeout;

            // Requested out of order so the result order comes from the due times
            await RequestTimeout<LateTimeout>(context, TimeSpan.FromMinutes(30));
            await RequestTimeout<EarlyTimeout>(context, TimeSpan.FromMinutes(10));
        }

        public Task Timeout(EarlyTimeout state, IMessageHandlerContext context) =>
            Data.FailEarlyTimeout ? throw new InvalidOperationException("Simulated failure") : Task.CompletedTask;

        public Task Timeout(LateTimeout state, IMessageHandlerContext context) => Task.CompletedTask;

        public async Task Handle(StartSimultaneous message, IMessageHandlerContext context)
        {
            for (var dueTime = 1; dueTime <= message.DueTimes; dueTime++)
            {
                for (var number = 0; number < message.Count; number++)
                {
                    await RequestTimeout(context, TimeSpan.FromMinutes(10 * dueTime), new NumberedTimeout { Number = number });
                }
            }
        }

        public Task Timeout(NumberedTimeout state, IMessageHandlerContext context) => Task.CompletedTask;
    }

    public class MyData : ContainSagaData
    {
        public string CorrId { get; set; }
        public bool FailEarlyTimeout { get; set; }
    }

    public class Start : ICommand
    {
        public string CorrId { get; set; }
        public bool FailEarlyTimeout { get; set; }
    }

    public class StartSimultaneous : ICommand
    {
        public string CorrId { get; set; }
        public int Count { get; set; }
        public int DueTimes { get; set; }
    }

    public class EarlyTimeout { }

    public class NumberedTimeout
    {
        public int Number { get; set; }
    }

    public class LateTimeout { }
}
