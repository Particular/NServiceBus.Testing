namespace NServiceBus.Testing.Tests.Saga
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class DiscountPolicySagaTests
    {
        [Test]
        public void DiscountTestWithActions()
        {
            decimal total = 600;

            Test.Saga<DiscountPolicy>()
                .ExpectSend<ProcessOrder>(m => Assert.That(() => m.Total, Is.EqualTo(total)))
                .When<SubmitOrder>(s => s.Handle, m => m.Total = total);
        }

        [Test]
        public void DiscountTestWithTimeout()
        {
            decimal total = 600;

            Test.Saga<DiscountPolicy>()
                .ExpectSend<ProcessOrder>(m => m.Total == total)
                .ExpectTimeoutToBeSetIn<SubmitOrder>((state, span) => span == TimeSpan.FromDays(7))
                .When((s, c) => s.Handle(new SubmitOrder
                {
                    Total = total
                }, c))
                .WhenHandlingTimeout<SubmitOrder>(m => m.Total = total)
                .ExpectSend<ProcessOrder>(m => m.Total == total)
                .ExpectTimeoutToBeSetIn<SubmitOrder>((state, span) => span == TimeSpan.FromDays(7))
                .When(s => s.Handle, new SubmitOrder
                {
                    Total = total
                });
        }

        [Test]
        public void RemoteOrder()
        {
            decimal total = 100;

            Test.Saga<DiscountPolicy>()
                .ExpectSendToDestination<ProcessOrder>((m, a) => m.Total == total && a == "remote.orderQueue")
                .ExpectTimeoutToBeSetIn<SubmitOrder>((state, span) => span == TimeSpan.FromDays(7))
                .When((s, c) => s.Handle(new SubmitOrder
                {
                    Total = total,
                    IsRemoteOrder = true
                }, c));
        }

        [Test]
        public void NotRemoteOrder()
        {
            decimal total = 100;

            Test.Saga<DiscountPolicy>()
                .ExpectNotSendToDestination<ProcessOrder>((m, a) => m.Total == total && a == "remote.orderQueue")
                .When((s, c) => s.Handle(new SubmitOrder
                {
                    Total = total,
                    IsRemoteOrder = false
                }, c));
        }

        [Test]
        public void RemoteOrderWithAssertions()
        {
            decimal total = 100;

            Test.Saga<DiscountPolicy>()
                .ExpectSendToDestination<ProcessOrder>((m, a) =>
                {
                    Assert.That(() => m.Total, Is.EqualTo(total));
                    Assert.That(() => a, Is.EqualTo("remote.orderQueue"));
                })
                .ExpectTimeoutToBeSetIn<SubmitOrder>((state, span) => span == TimeSpan.FromDays(7))
                .When((s, c) => s.Handle(new SubmitOrder
                {
                    Total = total,
                    IsRemoteOrder = true
                }, c));
        }

        [Test]
        public void DiscountTestWithSpecificTimeout()
        {
            Test.Saga<DiscountPolicy>()
                .ExpectSend<ProcessOrder>(m => m.Total == 500)
                .ExpectTimeoutToBeSetIn<SubmitOrder>((state, span) => span == TimeSpan.FromDays(7))
                .When((s, c) => s.Handle(new SubmitOrder
                {
                    Total = 500
                }, c))
                .ExpectSend<ProcessOrder>(m => m.Total == 400)
                .ExpectTimeoutToBeSetIn<SubmitOrder>((state, span) => span == TimeSpan.FromDays(7))
                .When((s, c) => s.Handle(new SubmitOrder
                {
                    Total = 400
                }, c))
                .ExpectSend<ProcessOrder>(m => m.Total == 300* 0.9M)
                .ExpectTimeoutToBeSetIn<SubmitOrder>((state, span) => span == TimeSpan.FromDays(7))
                .When((s, c) => s.Handle(new SubmitOrder
                {
                    Total = 300
                }, c))
                .WhenHandlingTimeout<SubmitOrder>()
                .ExpectSend<ProcessOrder>(m => m.Total == 200* 0.9M)
                .ExpectTimeoutToBeSetIn<SubmitOrder>((state, span) => span == TimeSpan.FromDays(7))
                .When((s, c) => s.Handle(new SubmitOrder
                {
                    Total = 200
                }, c));
        }

        [Test]
        public void DiscountTest()
        {
            decimal total = 600;

            Test.Saga<DiscountPolicy>()
                .ExpectSend<ProcessOrder>(m => m.Total == total)
                .ExpectTimeoutToBeSetIn<SubmitOrder>((state, span) => span == TimeSpan.FromDays(7))
                .When((s, c) => s.Handle(new SubmitOrder
                {
                    Total = total
                }, c))
                .ExpectSend<ProcessOrder>(m => m.Total == total * 0.9M)
                .ExpectTimeoutToBeSetIn<SubmitOrder>((state, span) => span == TimeSpan.FromDays(7))
                .When<SubmitOrder>(s => s.Handle, m => m.Total = total);
        }

        public class DiscountPolicy : NServiceBus.Saga<DiscountPolicyData>,
            IAmStartedByMessages<SubmitOrder>,
            IHandleTimeouts<SubmitOrder>
        {
            public async Task Handle(SubmitOrder message, IMessageHandlerContext context)
            {
                Data.CustomerId = message.CustomerId;
                Data.RunningTotal += message.Total;

                if (message.IsRemoteOrder)
                {
                    await ProcessExternalOrder(message, context);
                }
                else if (Data.RunningTotal >= 1000)
                {
                    await ProcessOrderWithDiscount(message, context);
                }
                else
                {
                    await ProcessOrder(message, context);
                }

                await RequestTimeout(context, TimeSpan.FromDays(7), message);
            }

            public Task Timeout(SubmitOrder state, IMessageHandlerContext context)
            {
                Data.RunningTotal -= state.Total;
                return Task.FromResult(0);
            }

            Task ProcessExternalOrder(SubmitOrder message, IMessageHandlerContext context)
            {
                return context.Send<ProcessOrder>("remote.orderQueue", m =>
                {
                    m.CustomerId = Data.CustomerId;
                    m.OrderId = message.OrderId;
                    m.Total = message.Total;
                });
            }

            Task ProcessOrder(SubmitOrder message, IMessageHandlerContext context)
            {
                return context.Send<ProcessOrder>(m =>
                {
                    m.CustomerId = Data.CustomerId;
                    m.OrderId = message.OrderId;
                    m.Total = message.Total;
                });
            }

            Task ProcessOrderWithDiscount(SubmitOrder message, IMessageHandlerContext context)
            {
                return context.Send<ProcessOrder>(m =>
                {
                    m.CustomerId = Data.CustomerId;
                    m.OrderId = message.OrderId;
                    m.Total = message.Total* 0.9M;
                });
            }

            protected override void ConfigureHowToFindSaga(SagaPropertyMapper<DiscountPolicyData> mapper)
            {
            }
        }

        public class SubmitOrder : IMessage
        {
            public Guid OrderId { get; set; }
            public Guid CustomerId { get; set; }
            public decimal Total { get; set; }
            public bool IsRemoteOrder { get; set; }
        }

        public class ProcessOrder : IMessage
        {
            public Guid OrderId { get; set; }
            public Guid CustomerId { get; set; }
            public decimal Total { get; set; }
        }

        public class DiscountPolicyData : IContainSagaData
        {
            public Guid CustomerId { get; set; }
            public decimal RunningTotal { get; set; }
            public Guid Id { get; set; }
            public string Originator { get; set; }
            public string OriginalMessageId { get; set; }
        }
    }
}