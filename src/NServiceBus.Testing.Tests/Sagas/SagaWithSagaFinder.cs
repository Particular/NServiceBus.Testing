namespace NServiceBus.Testing.Tests.Sagas
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Extensibility;
    using NServiceBus.Sagas;
    using NUnit.Framework;
    using Persistence;

    [TestFixture]
    public class SagaWithSagaFinder
    {
        [Test]
        public async Task TestSagaWithSagaFinder()
        {
            var testableSaga = new TestableSaga<ShippingPolicy, ShippingPolicyData>(sagaFinders: [typeof(SagaFinder)]);

            var placeResult = await testableSaga.Handle(new OrderPlaced { OrderId = "abc" });
            var billResult = await testableSaga.Handle(new OrderBilled { OrderId = "abc" });

            Assert.That(placeResult.Completed, Is.False);
            Assert.That(billResult.Completed, Is.False);

            // Snapshots of data should still be assertable even after multiple operations have occurred.
            Assert.That(placeResult.SagaDataSnapshot.OrderId, Is.EqualTo("abc"));
            Assert.That(placeResult.SagaDataSnapshot.Placed, Is.True);
            Assert.That(placeResult.SagaDataSnapshot.Billed, Is.False);

            var noResults = await testableSaga.AdvanceTime(TimeSpan.FromMinutes(10));
            Assert.That(noResults.Length, Is.EqualTo(0));

            var timeoutResults = await testableSaga.AdvanceTime(TimeSpan.FromHours(1));

            Assert.That(timeoutResults.Length, Is.EqualTo(1));

            var shipped = timeoutResults.First().FindPublishedMessage<OrderShipped>();
            Assert.That(shipped.OrderId == "abc");
        }

        public class ShippingPolicy : Saga<ShippingPolicyData>,
            IAmStartedByMessages<OrderPlaced>,
#pragma warning disable NSB0006
            //using a saga finder — no explicit mapping for OrderBilled
            IAmStartedByMessages<OrderBilled>,
#pragma warning restore NSB0006
            IHandleTimeouts<ShippingDelay>
        {
            protected override void ConfigureHowToFindSaga(SagaPropertyMapper<ShippingPolicyData> mapper)
            {
                mapper.MapSaga(saga => saga.OrderId)
                    .ToMessage<OrderPlaced>(msg => msg.OrderId);
            }

            public Task Handle(OrderPlaced message, IMessageHandlerContext context)
            {
                Data.Placed = true;
                return TimeToShip(context);
            }
            public Task Handle(OrderBilled message, IMessageHandlerContext context)
            {
                Data.Billed = true;
                return TimeToShip(context);
            }
            public async Task TimeToShip(IMessageHandlerContext context)
            {
                if (Data.Placed && Data.Billed)
                {
                    await RequestTimeout<ShippingDelay>(context, TimeSpan.FromMinutes(15));
                }
            }

            public async Task Timeout(ShippingDelay state, IMessageHandlerContext context)
            {
                await context.Publish(new OrderShipped { OrderId = Data.OrderId });
                MarkAsComplete();
            }
        }

        public class ShippingPolicyData : ContainSagaData
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

        public class ShippingDelay { }

        public class SagaFinder : ISagaFinder<ShippingPolicyData, OrderBilled>
        {
            public Task<ShippingPolicyData> FindBy(OrderBilled message, ISynchronizedStorageSession storageSession,
                IReadOnlyContextBag context, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(new ShippingPolicyData());
            }
        }
    }
}
