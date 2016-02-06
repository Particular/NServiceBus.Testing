namespace NServiceBus.Testing.Tests
{
    using System;
    using NUnit.Framework;
    using Saga;

    [TestFixture]
    class SagaTests : BaseTests
    {
        [Test]
        public void MySaga()
        {
            Test.Saga<MySaga>()
                .ExpectReplyToOriginator<ResponseToOriginator>()
                .ExpectTimeoutToBeSetIn<StartsSaga>((state, span) => span == TimeSpan.FromDays(7))
                .ExpectPublish<Event>()
                .ExpectSend<Command>()
                .When(s => s.Handle(new StartsSaga()))
                .ExpectPublish<Event>()
                .WhenSagaTimesOut()
                .AssertSagaCompletionIs(true);
        }

        [Test]
        public void MySagaWithActions()
        {
            Test.Saga<MySaga>()
                .ExpectTimeoutToBeSetIn<StartsSaga>(
                    (state, span) => Assert.That(() => span, Is.EqualTo(TimeSpan.FromDays(7))))
                .When(s => s.Handle(new StartsSaga()));
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
                .When(s => s.Handle(new MyRequest()));
        }

        [Test]
        public void DiscountTest()
        {
            decimal total = 600;

            Test.Saga<DiscountPolicy>()
                .ExpectSend<ProcessOrder>(m => m.Total == total)
                .ExpectTimeoutToBeSetIn<SubmitOrder>((state, span) => span == TimeSpan.FromDays(7))
                .When(s => s.Handle(new SubmitOrder { Total = total }))
                .ExpectSend<ProcessOrder>(m => m.Total == total * (decimal)0.9)
                .ExpectTimeoutToBeSetIn<SubmitOrder>((state, span) => span == TimeSpan.FromDays(7))
                .When(s => s.Handle(new SubmitOrder { Total = total }));
        }

        [Test]
        public void DiscountTestWithActions()
        {
            decimal total = 600;

            Test.Saga<DiscountPolicy>()
                .ExpectSend<ProcessOrder>(m => Assert.That(() => m.Total, Is.EqualTo(total)))
                .When(s => s.Handle(new SubmitOrder { Total = total }));
        }

        [Test]
        public void DiscountTestWithTimeout()
        {
            decimal total = 600;

            Test.Saga<DiscountPolicy>()
                .ExpectSend<ProcessOrder>(m => m.Total == total)
                .ExpectTimeoutToBeSetIn<SubmitOrder>((state, span) => span == TimeSpan.FromDays(7))
                .When(s => s.Handle(new SubmitOrder { Total = total }))
                .WhenSagaTimesOut()
                .ExpectSend<ProcessOrder>(m => m.Total == total)
                .ExpectTimeoutToBeSetIn<SubmitOrder>((state, span) => span == TimeSpan.FromDays(7))
                .When(s => s.Handle(new SubmitOrder { Total = total }));
        }


        [Test]
        public void RemoteOrder()
        {
            decimal total = 100;

            Test.Saga<DiscountPolicy>()
                .ExpectSendToDestination<ProcessOrder>((m, a) => m.Total == total && a.Queue == "remote.orderQueue")
                .ExpectTimeoutToBeSetIn<SubmitOrder>((state, span) => span == TimeSpan.FromDays(7))
                .When(s => s.Handle(new SubmitOrder { Total = total, IsRemoteOrder = true }));
        }

        [Test]
        public void RemoteOrderWithAssertions()
        {
            decimal total = 100;

            Test.Saga<DiscountPolicy>()
                .ExpectSendToDestination<ProcessOrder>((m, a) =>
                {
                    Assert.That(() => m.Total, Is.EqualTo(total));
                    Assert.That(() => a.Queue, Is.EqualTo("remote.orderQueue"));
                })
                .ExpectTimeoutToBeSetIn<SubmitOrder>((state, span) => span == TimeSpan.FromDays(7))
                .When(s => s.Handle(new SubmitOrder { Total = total, IsRemoteOrder = true }));
        }

        [Test]
        public void DiscountTestWithSpecificTimeout()
        {
            Test.Saga<DiscountPolicy>()
                .ExpectSend<ProcessOrder>(m => m.Total == 500)
                .ExpectTimeoutToBeSetIn<SubmitOrder>((state, span) => span == TimeSpan.FromDays(7))
                .When(s => s.Handle(new SubmitOrder { Total = 500 }))
                .ExpectSend<ProcessOrder>(m => m.Total == 400)
                .ExpectTimeoutToBeSetIn<SubmitOrder>((state, span) => span == TimeSpan.FromDays(7))
                .When(s => s.Handle(new SubmitOrder { Total = 400 }))
                .ExpectSend<ProcessOrder>(m => m.Total == 300 * (decimal)0.9)
                .ExpectTimeoutToBeSetIn<SubmitOrder>((state, span) => span == TimeSpan.FromDays(7))
                .When(s => s.Handle(new SubmitOrder { Total = 300 }))
                .WhenSagaTimesOut()
                .ExpectSend<ProcessOrder>(m => m.Total == 200)
                .ExpectTimeoutToBeSetIn<SubmitOrder>((state, span) => span == TimeSpan.FromDays(7))
                .When(s => s.Handle(new SubmitOrder { Total = 200 }));
        }

        [Test]
        public void TestNullReferenceException()
        {
            Test.Initialize();
            var saga = new MySaga();
            Assert.DoesNotThrow(() => Test.Saga(saga));
        }

        [Test]
        public void ShouldFailExpectForwardCurrentMessageToIfMessageForwardedToUnexpectedDestination()
        {
            Assert.Throws<Exception>(() => Test.Saga<MySaga>()
                .ExpectForwardCurrentMessageTo(dest => dest == "expectedDestination")
                .When(s => s.Handle(new StartsSaga())));
        }

        [Test]
        public void ShouldPassExpectForwardCurrentMessageToIfMessageForwardedToExpectedDestination()
        {
            Test.Saga<MySaga>()
                .ExpectForwardCurrentMessageTo(dest => dest == "forwardingDestination")
                .When(s => s.Handle(new StartsSaga()));
        }

        [Test]
        public void ShouldFailExpectNotForwardCurrentMessageToIfMessageForwardedToExpectedDestination()
        {
            Assert.Throws<Exception>(() => Test.Saga<MySaga>()
                .ExpectNotForwardCurrentMessageTo(dest => dest == "forwardingDestination")
                .When(s => s.Handle(new StartsSaga())));
        }

        [Test]
        public void ShouldPassExpectNotForwardCurrentMessageToIfMessageForwardedToUnexpectedDestination()
        {
            Test.Saga<MySaga>()
                .ExpectNotForwardCurrentMessageTo(dest => dest == "expectedDestination")
                .When(s => s.Handle(new StartsSaga()));
        }

        [Test]
        public void ShouldPassAssertSagaData_specifying_saga_data_type()
        {
            var orderId = Guid.NewGuid();
            var customerId = Guid.NewGuid();
            Test.Saga<DiscountPolicy>()
                .AssertSagaData<DiscountPolicyData>(state => state.RunningTotal == 0M)
                .When(s => s.Handle(new SubmitOrder { CustomerId = customerId, OrderId = orderId, Total = 123.99M }))
                .AssertSagaData<DiscountPolicyData>(state => state.CustomerId == customerId && state.RunningTotal == 123.99M);
        }

        [Test]
        public void ShouldPassAssertSagaData_without_specifying_saga_data_type()
        {
            var customerId = Guid.NewGuid();
            Test.Saga<DiscountPolicy, DiscountPolicyData>()
                .AssertSagaData(state => state.RunningTotal == 0M)
                .When(s => s.Handle(new SubmitOrder { CustomerId = customerId, OrderId = Guid.NewGuid(), Total = 123.99M }))
                .AssertSagaData(state => state.CustomerId == customerId && state.RunningTotal == 123.99M)
                .When(s => s.Handle(new SubmitOrder { CustomerId = customerId, OrderId = Guid.NewGuid(), Total = 100.00M }))
                .AssertSagaData(state => state.CustomerId == customerId && state.RunningTotal == 223.99M);
        }
    }


    public class SagaThatDoesAReply : Saga<SagaThatDoesAReply.SagaThatDoesAReplyData>,
        IHandleMessages<MyRequest>
    {

        public class SagaThatDoesAReplyData : ContainSagaData
        {
        }

        public void Handle(MyRequest myRequest)
        {
            Bus.Reply(new MyReply());
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<SagaThatDoesAReplyData> mapper)
        {
        }
    }

    public class MyRequest
    {
    }


    public class MyReply
    {
    }

    public class MySagaWithInterface : Saga<MySagaWithInterface.MySagaDataWithInterface>,
        IAmStartedByMessages<StartsSagaWithInterface>
    {
        public class MySagaDataWithInterface : ContainSagaData
        {

        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<MySagaDataWithInterface> mapper)
        {
        }

        public void Handle(StartsSagaWithInterface message)
        {
            if (message.Foo == "Hello")
            {
                Bus.Send<Command>(null);
            }
        }
    }

    public class MySaga : Saga<MySagaData>,
                          IAmStartedByMessages<StartsSaga>,
                          IHandleTimeouts<StartsSaga>
    {
        public void Handle(StartsSaga message)
        {
            ReplyToOriginator(new ResponseToOriginator());
            Bus.Publish<Event>();
            Bus.Send<Command>(null);
            Bus.ForwardCurrentMessageTo("forwardingDestination");
            RequestTimeout(TimeSpan.FromDays(7), message);
        }

        public void Timeout(StartsSaga state)
        {
            Bus.Publish<Event>();
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

    public class DiscountPolicy : Saga<DiscountPolicyData>,
                                  IAmStartedByMessages<SubmitOrder>,
                                  IHandleTimeouts<SubmitOrder>
    {
        public void Handle(SubmitOrder message)
        {
            Data.CustomerId = message.CustomerId;
            Data.RunningTotal += message.Total;

            if (message.IsRemoteOrder)
                ProcessExternalOrder(message);
            else if (Data.RunningTotal >= 1000)
                ProcessOrderWithDiscount(message);
            else
                ProcessOrder(message);

            RequestTimeout(TimeSpan.FromDays(7), message);
        }

        private void ProcessExternalOrder(SubmitOrder message)
        {
            Bus.Send<ProcessOrder>("remote.orderQueue", m =>
                                                            {
                                                                m.CustomerId = Data.CustomerId;
                                                                m.OrderId = message.OrderId;
                                                                m.Total = message.Total;
                                                            });
        }

        public void Timeout(SubmitOrder state)
        {
            Data.RunningTotal -= state.Total;
        }

        private void ProcessOrder(SubmitOrder message)
        {
            Bus.Send<ProcessOrder>(m =>
                                       {
                                           m.CustomerId = Data.CustomerId;
                                           m.OrderId = message.OrderId;
                                           m.Total = message.Total;
                                       });
        }

        private void ProcessOrderWithDiscount(SubmitOrder message)
        {
            Bus.Send<ProcessOrder>(m =>
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
        public Guid Id { get; set; }
        public string Originator { get; set; }
        public string OriginalMessageId { get; set; }

        public Guid CustomerId { get; set; }
        public decimal RunningTotal { get; set; }
    }
}
