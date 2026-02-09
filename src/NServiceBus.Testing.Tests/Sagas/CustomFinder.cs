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
    public async Task TestSagaWithCustomFinder()
    {
        var testableSaga = new TestableSaga<SagaWithCustomFinder, CustomFinderSagaData>();

        var placeResult = await testableSaga.Handle(new OrderPlaced { OrderId = "abc" });

        var exception = Assert.ThrowsAsync<NotSupportedException>(async () => await testableSaga.Handle(new OrderBilled { OrderId = "abc" }));

        Assert.Multiple(() =>
        {
            Assert.That(placeResult.Completed, Is.False);
            Assert.That(placeResult.SagaDataSnapshot.Placed, Is.True);
            Assert.That(placeResult.SagaDataSnapshot.Billed, Is.False);
            Assert.That(exception?.Message, Contains.Substring("custom saga finder"));
        });
    }

    public class SagaWithCustomFinder : Saga<CustomFinderSagaData>,
        IAmStartedByMessages<OrderPlaced>,
        IAmStartedByMessages<OrderBilled>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<CustomFinderSagaData> mapper)
        {
            mapper.MapSaga(saga => saga.OrderId)
                .ToMessage<OrderPlaced>(msg => msg.OrderId);

            mapper.ConfigureFinderMapping<OrderBilled, MyFinder>();
        }

        public class MyFinder : ISagaFinder<CustomFinderSagaData, OrderBilled>
        {
            public Task<CustomFinderSagaData> FindBy(OrderBilled message, ISynchronizedStorageSession storageSession, IReadOnlyContextBag context, CancellationToken cancellationToken = new CancellationToken()) => throw new NotImplementedException();
        }

        public Task Handle(OrderPlaced message, IMessageHandlerContext context) => Task.FromResult(Data.Placed = true);

        public Task Handle(OrderBilled message, IMessageHandlerContext context) => Task.FromResult(Data.Billed = true);
    }

    public class CustomFinderSagaData : ContainSagaData
    {
        public string OrderId { get; set; }
        public bool Placed { get; set; }
        public bool Billed { get; set; }
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