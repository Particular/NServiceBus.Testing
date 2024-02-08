namespace NServiceBus.Testing.Tests.Sagas
{
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class TwoSagas
    {
        [Test]
        public async Task TestWithDifferentCorrelationIds()
        {
            var testableSaga = new TestableSaga<ShippingPolicy, ShippingPolicyData>();

            var placeResultA = await testableSaga.Handle(new OrderPlaced { OrderId = "abc" });
            var billedResultB = await testableSaga.Handle(new OrderBilled { OrderId = "def" });

            Assert.That(placeResultA.Completed, Is.False);
            Assert.That(billedResultB.Completed, Is.False);
            Assert.That(placeResultA.SagaId, Is.Not.EqualTo(billedResultB.SagaId));

            var billedResultA = await testableSaga.Handle(new OrderBilled { OrderId = "abc" });
            var shipped = billedResultA.FindPublishedMessage<OrderShipped>();

            Assert.That(shipped.OrderId == "abc");
        }

        public class ShippingPolicy : Saga<ShippingPolicyData>,
            IAmStartedByMessages<OrderPlaced>,
            IAmStartedByMessages<OrderBilled>
        {
            protected override void ConfigureHowToFindSaga(SagaPropertyMapper<ShippingPolicyData> mapper)
            {
                mapper.MapSaga(saga => saga.OrderId)
                    .ToMessage<OrderPlaced>(msg => msg.OrderId)
                    .ToMessage<OrderBilled>(msg => msg.OrderId);
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
                    await context.Publish(new OrderShipped { OrderId = Data.OrderId });
                }
            }
        }

        public class ShippingPolicyData : ContainSagaData
        {
            public string OrderId { get; set; }
            public bool Placed { get; set; }
            public bool Billed { get; set; }
            public bool HeaderMessageReceived { get; set; }
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
}
