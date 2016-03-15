namespace NServiceBus.Testing.Tests.Saga
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
        public void TestNullReferenceException()
        {
            var saga = new MySaga();
            Assert.DoesNotThrow(() => Test.Saga(saga));
        }

        [Test]
        public void SetIncomingHeader()
        {
            const string customHeaderKey = "custom-header";
            const string expectedHeaderValue = "header value";
            var containsCustomHeader = false;
            string receivedHeaderValue = null;

            var saga = new CustomSaga<MyRequest, MySagaData>
            {
                HandlerAction = (request, context, data) =>
                {
                    containsCustomHeader = context.MessageHeaders.TryGetValue(customHeaderKey, out receivedHeaderValue);
                    return Task.FromResult(0);
                }
            };

            Test.Saga(saga)
                .SetIncomingHeader(customHeaderKey, expectedHeaderValue)
                .When((s, c) => s.Handle(new MyRequest(), c));

            Assert.IsTrue(containsCustomHeader);
            Assert.AreEqual(expectedHeaderValue, receivedHeaderValue);
        }
    }

    public class MyRequest
    {
        public bool ShouldReply { get; set; }
        public string String { get; set; }
    }

    public class MyReply
    {
        public string String { get; set; }
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

    public class CustomSaga<TMessage, TSagaData> : NServiceBus.Saga<TSagaData>, IHandleMessages<TMessage> where TSagaData : IContainSagaData, new()
    {
        public Func<TMessage, IMessageHandlerContext, TSagaData, Task> HandlerAction { get; set; }

        public Task Handle(TMessage message, IMessageHandlerContext context)
        {
            return HandlerAction(message, context, Data);
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<TSagaData> mapper)
        {
        }
    }
}