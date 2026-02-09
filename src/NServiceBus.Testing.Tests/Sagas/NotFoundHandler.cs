namespace NServiceBus.Testing.Tests.Sagas;

using System;
using System.Threading.Tasks;
using NUnit.Framework;

[TestFixture]
public class NotFoundHandler
{
    [Test]
    public async Task TestSagaWithNotFoundHandler()
    {
        var testableSaga = new TestableSaga<SagaWithCustomFinder, CustomFinderSagaData>();

        var placeResult = await testableSaga.Handle(new OrderPlaced { OrderId = "abc" });

        var exception = Assert.ThrowsAsync<Exception>(async () => await testableSaga.Handle(new OrderBilled { OrderId = "abc" }));

        Assert.Multiple(() =>
        {
            Assert.That(placeResult.Completed, Is.True);
            Assert.That(placeResult.SagaDataSnapshot.Placed, Is.True);
            Assert.That(placeResult.SagaDataSnapshot.Billed, Is.False);
            Assert.That(exception?.Message, Contains.Substring("Saga not found").And.Contains("not allowed to start the saga"));
        });
    }

    public class SagaWithCustomFinder : Saga<CustomFinderSagaData>,
        IAmStartedByMessages<OrderPlaced>,
        IHandleMessages<OrderBilled>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<CustomFinderSagaData> mapper)
        {
            mapper.MapSaga(saga => saga.OrderId)
                .ToMessage<OrderPlaced>(msg => msg.OrderId)
                .ToMessage<OrderBilled>(msg => msg.OrderId);

            mapper.ConfigureNotFoundHandler<NotFoundHandlerClass>();
        }

        public Task Handle(OrderPlaced message, IMessageHandlerContext context)
        {
            Data.Placed = true;
            MarkAsComplete();
            return Task.CompletedTask;
        }

        public async Task Handle(OrderBilled message, IMessageHandlerContext context)
        {
            Data.Placed = true;
            await context.Send(new OrderBilled { OrderId = message.OrderId });
            MarkAsComplete(); // Saga won't be around to get message
        }
    }

    public class NotFoundHandlerClass : ISagaNotFoundHandler
    {
        public Task Handle(object message, IMessageProcessingContext context) => Task.CompletedTask;
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
}