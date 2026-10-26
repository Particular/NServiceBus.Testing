namespace NServiceBus.Testing.Tests.Sagas;

using System;
using System.Threading.Tasks;
using NUnit.Framework;

[TestFixture]
public class HandlerExceptions
{
    [Test]
    public void SynchronousExceptionFromHandlerShouldNotBeWrapped()
    {
        var testableSaga = new TestableSaga<MySaga, MyData>();

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await testableSaga.Handle(new ThrowSynchronously { CorrId = "abc" }));

        Assert.That(exception.Message, Is.EqualTo("Synchronous failure"));
    }

    [Test]
    public void AsynchronousExceptionFromHandlerShouldNotBeWrapped()
    {
        var testableSaga = new TestableSaga<MySaga, MyData>();

        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await testableSaga.Handle(new ThrowAsynchronously { CorrId = "abc" }));

        Assert.That(exception.Message, Is.EqualTo("Asynchronous failure"));
    }

    public class MySaga : Saga<MyData>,
        IAmStartedByMessages<ThrowSynchronously>,
        IAmStartedByMessages<ThrowAsynchronously>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<MyData> mapper) =>
            mapper.MapSaga(saga => saga.CorrId)
                .ToMessage<ThrowSynchronously>(msg => msg.CorrId)
                .ToMessage<ThrowAsynchronously>(msg => msg.CorrId);

        public Task Handle(ThrowSynchronously message, IMessageHandlerContext context) => throw new InvalidOperationException("Synchronous failure");

        public async Task Handle(ThrowAsynchronously message, IMessageHandlerContext context)
        {
            await Task.Yield();
            throw new InvalidOperationException("Asynchronous failure");
        }
    }


    public class MyData : ContainSagaData
    {
        public string CorrId { get; set; }
    }

    public class ThrowSynchronously : ICommand
    {
        public string CorrId { get; set; }
    }

    public class ThrowAsynchronously : ICommand
    {
        public string CorrId { get; set; }
    }
}
