namespace NServiceBus.Testing.Tests.Sagas;

using System;
using System.Threading;
using System.Threading.Tasks;
using Extensibility;
using NServiceBus.Sagas;
using NUnit.Framework;
using Persistence;

[TestFixture]
public class CustomFinder
{
    [Test]
    public void TestSagaWithCustomFinderForMessageStartingTheSaga()
    {
        var exception = Assert.Throws<Exception>(() => new TestableSaga<SagaWithCustomFinderStarting, SagaWithCustomFinderStarting.SagaData>());

        Assert.That(exception?.Message, Contains.Substring("Message type OrderBilled can start the saga SagaWithCustomFinderStarting (the saga implements IAmStartedByMessages<OrderBilled>) but does not map that message to saga data"));
    }

    [Test]
    public async Task TestSagaWithCustomFinderForMessageNotStartingTheSaga()
    {
        var testableSaga = new TestableSaga<SagaWithCustomFinderNotStarting, SagaWithCustomFinderNotStarting.SagaData>();

        var placeResult = await testableSaga.Handle(new OrderPlaced { OrderId = "abc" });

        var exception = Assert.ThrowsAsync<Exception>(async () => await testableSaga.Handle(new OrderBilled { OrderId = "abc" }));

        Assert.Multiple(() =>
        {
            Assert.That(placeResult.Completed, Is.False);
            Assert.That(placeResult.SagaDataSnapshot.Placed, Is.True);
            Assert.That(placeResult.SagaDataSnapshot.Billed, Is.False);
            Assert.That(exception?.Message, Contains.Substring("No mapped value found from message, could not look up saga data"));
        });
    }

    public class SagaWithCustomFinderStarting : Saga<SagaWithCustomFinderStarting.SagaData>,
        IAmStartedByMessages<OrderPlaced>,
#pragma warning disable NSB0006
        IAmStartedByMessages<OrderBilled>
#pragma warning restore NSB0006
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<SagaData> mapper) =>
            mapper.MapSaga(saga => saga.OrderId)
                .ToMessage<OrderPlaced>(msg => msg.OrderId);

        public class MyFinder : ISagaFinder<SagaData, OrderBilled>
        {
            public Task<SagaData> FindBy(OrderBilled message, ISynchronizedStorageSession storageSession, IReadOnlyContextBag context, CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();
        }

        public Task Handle(OrderPlaced message, IMessageHandlerContext context) => Task.FromResult(Data.Placed = true);

        public Task Handle(OrderBilled message, IMessageHandlerContext context) => Task.FromResult(Data.Billed = true);

        public class SagaData : ContainSagaData
        {
            public string OrderId { get; set; }
            public bool Placed { get; set; }
            public bool Billed { get; set; }
        }
    }

    public class SagaWithCustomFinderNotStarting : Saga<SagaWithCustomFinderNotStarting.SagaData>,
        IAmStartedByMessages<OrderPlaced>,
        IHandleMessages<OrderBilled>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<SagaData> mapper) =>
            mapper.MapSaga(saga => saga.OrderId)
                .ToMessage<OrderPlaced>(msg => msg.OrderId);

        public class MyFinder : ISagaFinder<SagaData, OrderBilled>
        {
            public Task<SagaData> FindBy(OrderBilled message, ISynchronizedStorageSession storageSession, IReadOnlyContextBag context, CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();
        }

        public Task Handle(OrderPlaced message, IMessageHandlerContext context) => Task.FromResult(Data.Placed = true);

        public Task Handle(OrderBilled message, IMessageHandlerContext context) => Task.FromResult(Data.Billed = true);

        public class SagaData : ContainSagaData
        {
            public string OrderId { get; set; }
            public bool Placed { get; set; }
            public bool Billed { get; set; }
        }
    }

    public class OrderPlaced : IEvent
    {
        public string OrderId { get; set; }
    }

    public class OrderBilled : IEvent
    {
        public string OrderId { get; set; }
    }

    public class OrderShipped : IEvent
    {
        public string OrderId { get; set; }
    }
}