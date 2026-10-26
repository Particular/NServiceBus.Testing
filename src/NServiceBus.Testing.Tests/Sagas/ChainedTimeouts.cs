namespace NServiceBus.Testing.Tests.Sagas;

using System;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

[TestFixture]
public class ChainedTimeouts
{
    [Test]
    public async Task ChainedTimeoutShouldBeScheduledFromFirstTimeoutDueTime()
    {
        var startTime = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc);
        var testableSaga = new TestableSaga<MySaga, MyData>(initialCurrentTime: startTime);

        await testableSaga.Handle(new Start { CorrId = "abc" });

        var firstResults = await testableSaga.AdvanceTime(TimeSpan.FromMinutes(25));
        var noResults = await testableSaga.AdvanceTime(TimeSpan.FromMinutes(4));
        var secondResults = await testableSaga.AdvanceTime(TimeSpan.FromMinutes(1));

        Assert.Multiple(() =>
        {
            Assert.That(firstResults.Single().HandledMessage.Type, Is.EqualTo(typeof(FirstTimeout)));
            Assert.That(noResults, Has.Length.EqualTo(0));
            Assert.That(secondResults.Single().HandledMessage.Type, Is.EqualTo(typeof(SecondTimeout)));
        });
    }

    [Test]
    public async Task ChainedTimeoutDueWithinAdvanceTimeShouldNotBeHandledInSameCall()
    {
        var startTime = new DateTime(2026, 1, 1, 8, 0, 0, DateTimeKind.Utc);
        var testableSaga = new TestableSaga<MySaga, MyData>(initialCurrentTime: startTime);

        await testableSaga.Handle(new Start { CorrId = "abc" });

        var firstResults = await testableSaga.AdvanceTime(TimeSpan.FromHours(1));
        var secondResults = await testableSaga.AdvanceTime(TimeSpan.Zero);

        Assert.Multiple(() =>
        {
            Assert.That(firstResults.Single().HandledMessage.Type, Is.EqualTo(typeof(FirstTimeout)));
            Assert.That(secondResults.Single().HandledMessage.Type, Is.EqualTo(typeof(SecondTimeout)));
            Assert.That(testableSaga.CurrentTime, Is.EqualTo(startTime.AddHours(1)));
        });
    }

    public class MySaga : Saga<MyData>,
        IAmStartedByMessages<Start>,
        IHandleTimeouts<FirstTimeout>,
        IHandleTimeouts<SecondTimeout>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<MyData> mapper) =>
            mapper.MapSaga(saga => saga.CorrId)
                .ToMessage<Start>(msg => msg.CorrId);

        public Task Handle(Start message, IMessageHandlerContext context) => RequestTimeout<FirstTimeout>(context, TimeSpan.FromMinutes(15));

        public Task Timeout(FirstTimeout state, IMessageHandlerContext context) => RequestTimeout<SecondTimeout>(context, TimeSpan.FromMinutes(15));

        public Task Timeout(SecondTimeout state, IMessageHandlerContext context) => Task.CompletedTask;
    }

    public class MyData : ContainSagaData
    {
        public string CorrId { get; set; }
    }

    public class Start : ICommand
    {
        public string CorrId { get; set; }
    }

    public class FirstTimeout { }

    public class SecondTimeout { }
}
