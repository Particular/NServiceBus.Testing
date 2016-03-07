namespace NServiceBus.Testing.Tests
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    class SagaTests
    {
        [Test]
        public void MySaga()
        {
            Test.Saga<MySaga>()
                .ExpectReplyToOriginator<ResponseToOriginator>()
                .ExpectTimeoutToBeSetIn<StartsSaga>((state, span) => span == TimeSpan.FromDays(7))
                .ExpectPublish<Event>()
                .ExpectSend<Command>()
                .When((s, c) => s.Handle(new StartsSaga(), c))
                .ExpectPublish<Event>()
                .WhenHandlingTimeout<StartsSaga>()
                .AssertSagaCompletionIs(true);
        }

        [Test]
        public void MySagaWithActions()
        {
            Test.Saga<MySaga>()
                .ExpectTimeoutToBeSetIn<StartsSaga>((state, span) => span == TimeSpan.FromDays(7))
                .When((s, c) => s.Handle(new StartsSaga(), c));
        }

        [Test]
        public void SagaThatIsStartedWithInterface()
        {
            Test.Saga<MySagaWithInterface>()
                .ExpectSend<Command>()
                .WhenHandling<StartsSagaWithInterface>(m => m.Foo = "Hello");
        }

        [Test]
        public void SagaThatDoesAReply()
        {
            Test.Saga<SagaThatDoesAReply>()
                .ExpectReply<MyReply>(reply => reply != null)
                .When((s, c) => s.Handle(new MyRequest(), c));
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
                .ExpectSend<ProcessOrder>(m => m.Total == total * (decimal)0.9)
                .ExpectTimeoutToBeSetIn<SubmitOrder>((state, span) => span == TimeSpan.FromDays(7))
                .When<SubmitOrder>(s => s.Handle, m => m.Total = total);
        }

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
                .ExpectSend<ProcessOrder>(m => m.Total == 300 * (decimal)0.9)
                .ExpectTimeoutToBeSetIn<SubmitOrder>((state, span) => span == TimeSpan.FromDays(7))
                .When((s, c) => s.Handle(new SubmitOrder
                {
                    Total = 300
                }, c))
                .WhenHandlingTimeout<SubmitOrder>()
                .ExpectSend<ProcessOrder>(m => m.Total == 200 * (decimal)0.9)
                .ExpectTimeoutToBeSetIn<SubmitOrder>((state, span) => span == TimeSpan.FromDays(7))
                .When((s, c) => s.Handle(new SubmitOrder
                {
                    Total = 200
                }, c));
        }

        [Test]
        public void TestNullReferenceException()
        {
            var saga = new MySaga();
            Assert.DoesNotThrow(() => Test.Saga(saga));
        }

        [Test]
        public void ShouldFailExpectForwardCurrentMessageToIfMessageForwardedToUnexpectedDestination()
        {
            Assert.Throws<ExpectationException>(() => Test.Saga<MySaga>()
                .ExpectForwardCurrentMessageTo(dest => dest == "expectedDestination")
                .When((s, c) => s.Handle(new StartsSaga(), c)));
        }

        [Test]
        public void ShouldPassExpectForwardCurrentMessageToIfMessageForwardedToExpectedDestination()
        {
            Test.Saga<MySaga>()
                .ExpectForwardCurrentMessageTo(dest => dest == "forwardingDestination")
                .When((s, c) => s.Handle(new StartsSaga(), c));
        }

        [Test]
        public void ShouldFailExpectNotForwardCurrentMessageToIfMessageForwardedToExpectedDestination()
        {
            Assert.Throws<ExpectationException>(() => Test.Saga<MySaga>()
                .ExpectNotForwardCurrentMessageTo(dest => dest == "forwardingDestination")
                .When((s, c) => s.Handle(new StartsSaga(), c)));
        }

        [Test]
        public void ShouldPassExpectNotForwardCurrentMessageToIfMessageForwardedToUnexpectedDestination()
        {
            Test.Saga<MySaga>()
                .ExpectNotForwardCurrentMessageTo(dest => dest == "expectedDestination")
                .When((s, c) => s.Handle(new StartsSaga(), c));
        }

        [Test]
        public void TimeoutInThePast()
        {
            var expected = DateTime.UtcNow.AddDays(-3);
            var message = new TheMessage
            {
                TimeoutAt = expected
            };

            Test.Saga<MyTimeoutSaga>()
                .ExpectTimeoutToBeSetAt<TheTimeout>((m, at) => at == expected)
                .When((s, c) => s.Handle(message, c));
        }

        [Test]
        public void TimeoutInThePastWithSendOnTimeout()
        {
            var message = new TheMessage
            {
                TimeoutAt = DateTime.UtcNow.AddDays(-3)
            };

            Test.Saga<MyTimeoutSaga>()
                .ExpectTimeoutToBeSetAt<TheTimeout>((m, at) => true)
                .When((s, c) => s.Handle(message, c))
                .ExpectSend<TheMessageSentAtTimeout>()
                .WhenHandlingTimeout<TheTimeout>();
        }

        [Test]
        public void TimeoutInTheFuture()
        {
            var expected = DateTime.UtcNow.AddDays(3);
            var message = new TheMessage
            {
                TimeoutAt = expected
            };

            Test.Saga<MyTimeoutSaga>()
                .ExpectTimeoutToBeSetAt<TheTimeout>((m, at) => at == expected)
                .When((s, c) => s.Handle(message, c));
        }

        [Test]
        public void ExpectHandleCurrentMessageLater()
        {
            Test.Saga<HandleInFutureSaga>()
                .ExpectHandleCurrentMessageLater()
                .WhenHandling<MyRequest>();
        }

        [Test]
        public void ExpectSendLocal()
        {
            Test.Saga<SendLocalSaga>()
                .ExpectSendLocal<SendLocalSaga.Message>(m => m.Property == "Property")
                .WhenHandling<SendLocalSaga.RequestSendLocal>();
        }

        [Test]
        public void ExpectNotSendLocal()
        {
            Test.Saga<SendLocalSaga>()
                .ExpectNotSendLocal<SendLocalSaga.Message>()
                .WhenHandling<SendLocalSaga.RequestNotSendLocal>();
        }
    }

    public class SendLocalSaga : NServiceBus.Saga<SendLocalSaga.SendLocalSagaData>,
        IHandleMessages<SendLocalSaga.RequestSendLocal>,
        IHandleMessages<SendLocalSaga.RequestNotSendLocal>
    {
        public Task Handle(RequestNotSendLocal message, IMessageHandlerContext context)
        {
            return Task.FromResult(0);
        }

        public Task Handle(RequestSendLocal message, IMessageHandlerContext context)
        {
            return context.SendLocal(new Message());
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<SendLocalSagaData> mapper)
        {
        }

        public class SendLocalSagaData : ContainSagaData
        {
        }

        public class RequestSendLocal
        {
        }

        public class RequestNotSendLocal
        {
        }

        public class Message
        {
            public string Property => "Property";
        }
    }

    public class HandleInFutureSaga : NServiceBus.Saga<HandleInFutureSaga.HandleInFutureSagaData>,
        IHandleMessages<MyRequest>
    {
        public Task Handle(MyRequest message, IMessageHandlerContext context)
        {
            return context.HandleCurrentMessageLater();
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<HandleInFutureSagaData> mapper)
        {
        }

        public class HandleInFutureSagaData : ContainSagaData
        {
        }
    }

    public class SagaThatDoesAReply : NServiceBus.Saga<SagaThatDoesAReply.SagaThatDoesAReplyData>,
        IHandleMessages<MyRequest>
    {
        public Task Handle(MyRequest message, IMessageHandlerContext context)
        {
            return context.Reply(new MyReply());
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<SagaThatDoesAReplyData> mapper)
        {
        }

        public class SagaThatDoesAReplyData : ContainSagaData
        {
        }
    }

    public class MyRequest
    {
        public bool ShouldReply { get; set; }
    }

    public class MyReply
    {
    }

    public class MySagaWithInterface : NServiceBus.Saga<MySagaWithInterface.MySagaDataWithInterface>,
        IAmStartedByMessages<StartsSagaWithInterface>
    {
        public async Task Handle(StartsSagaWithInterface message, IMessageHandlerContext context)
        {
            if (message.Foo == "Hello")
            {
                await context.Send<Command>(m => { });
            }
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<MySagaDataWithInterface> mapper)
        {
        }

        public class MySagaDataWithInterface : ContainSagaData
        {
        }
    }

    public class MySaga : NServiceBus.Saga<MySagaData>,
        IAmStartedByMessages<StartsSaga>,
        IHandleTimeouts<StartsSaga>
    {
        public async Task Handle(StartsSaga message, IMessageHandlerContext context)
        {
            await ReplyToOriginator(context, new ResponseToOriginator());
            await context.Publish<Event>();
            await context.Send<Command>(s => { });
            await context.ForwardCurrentMessageTo("forwardingDestination");
            await RequestTimeout(context, TimeSpan.FromDays(7), message);
        }

        public async Task Timeout(StartsSaga state, IMessageHandlerContext context)
        {
            await context.Publish<Event>();
            MarkAsComplete();
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<MySagaData> mapper)
        {
        }
    }

    public class MySagaData : IContainSagaData
    {
        public Guid Id { get; set; }
        public string Originator { get; set; }
        public string OriginalMessageId { get; set; }
    }

    public interface StartsSagaWithInterface : IEvent
    {
        string Foo { get; set; }
    }

    public class StartsSaga : ICommand
    {
    }

    public class ResponseToOriginator : IMessage
    {
    }

    public interface Event : IEvent
    {
    }

    public class Command : ICommand
    {
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

        private Task ProcessExternalOrder(SubmitOrder message, IMessageHandlerContext context)
        {
            return context.Send<ProcessOrder>("remote.orderQueue", m =>
            {
                m.CustomerId = Data.CustomerId;
                m.OrderId = message.OrderId;
                m.Total = message.Total;
            });
        }

        private Task ProcessOrder(SubmitOrder message, IMessageHandlerContext context)
        {
            return context.Send<ProcessOrder>(m =>
            {
                m.CustomerId = Data.CustomerId;
                m.OrderId = message.OrderId;
                m.Total = message.Total;
            });
        }

        private Task ProcessOrderWithDiscount(SubmitOrder message, IMessageHandlerContext context)
        {
            return context.Send<ProcessOrder>(m =>
            {
                m.CustomerId = Data.CustomerId;
                m.OrderId = message.OrderId;
                m.Total = message.Total * (decimal)0.9;
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

    public class MyTimeoutSaga : NServiceBus.Saga<MyTimeoutData>,
        IAmStartedByMessages<TheMessage>,
        IHandleTimeouts<TheTimeout>
    {
        public Task Handle(TheMessage message, IMessageHandlerContext context)
        {
            return RequestTimeout<TheTimeout>(context, message.TimeoutAt);
        }

        public async Task Timeout(TheTimeout state, IMessageHandlerContext context)
        {
            await context.Send(new TheMessageSentAtTimeout());
            MarkAsComplete();
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<MyTimeoutData> mapper)
        {
        }
    }

    public class MyTimeoutData : IContainSagaData
    {
        public Guid Id { get; set; }
        public string Originator { get; set; }
        public string OriginalMessageId { get; set; }
    }

    public class TheMessage : IMessage
    {
        public DateTime TimeoutAt { get; set; }
    }

    public class TheTimeout : IMessage
    {
    }

    public class TheMessageSentAtTimeout : IMessage
    {
    }
}