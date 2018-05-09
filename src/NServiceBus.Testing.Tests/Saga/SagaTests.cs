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
                .WhenHandling(new StartsSaga())
                .ExpectPublish<Event>()
                .ExpectSagaCompleted()
                .WhenHandlingTimeout<StartsSaga>();
        }

        [Test]
        public void MySagaWithActions()
        {
            Test.Saga<MySaga>()
                .ExpectTimeoutToBeSetIn<StartsSaga>((state, span) => span == TimeSpan.FromDays(7))
                .WhenHandling(new StartsSaga());
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
                .WhenHandling(new MyRequest());

            Assert.IsTrue(containsCustomHeader);
            Assert.AreEqual(expectedHeaderValue, receivedHeaderValue);
        }

        [Test]
        public void ConfigureHandlerContext()
        {
            var messageId = Guid.NewGuid().ToString();
            var replyToAddress = "0118 999 881 999 119 725 3";
            TestableMessageHandlerContext configuredContextInstance = null;
            IMessageHandlerContext receivedContextInstance = null;

            Test.Saga<CustomSaga<MyRequest, MySagaData>>()
                .WithExternalDependencies(s =>
                    s.HandlerAction = (request, context, data) =>
                    {
                        receivedContextInstance = context;
                        return Task.FromResult(0);
                    })
                .ConfigureHandlerContext(c =>
                {
                    c.MessageId = messageId;
                    c.ReplyToAddress = replyToAddress;
                    configuredContextInstance = c;
                })
                .When<MyRequest>(s => s.Handle);

            Assert.AreEqual(messageId, receivedContextInstance.MessageId);
            Assert.AreEqual(replyToAddress, receivedContextInstance.ReplyToAddress);
            Assert.AreSame(receivedContextInstance, configuredContextInstance);
        }

        [Test]
        public void ShouldInvokeAllHandlerMethodsWhenHandlingSubclassedMessage()
        {
            var saga = new MessageHierarchySaga();
            Test.Saga(saga)
                .WhenHandling<BaseClassImplementingMessage>();

            Assert.IsTrue(saga.BaseClassMessageHandlerInvoked);
            Assert.IsTrue(saga.BaseClassImplementingMessageHandlerInvoked);
        }

        [Test]
        public void ShouldOnlyInvokeBaseClassHandlerMethofWhenHandlingBaseClassMessage()
        {
            var saga = new MessageHierarchySaga();
            Test.Saga(saga)
                .WhenHandling<BaseClassMessage>();

            Assert.IsTrue(saga.BaseClassMessageHandlerInvoked);
            Assert.IsFalse(saga.BaseClassImplementingMessageHandlerInvoked);
        }

        [Test]
        public void ShouldInvokeBaseClassHandlerForSubclassedMessages()
        {
            var handlerInvoked = false;
            var saga = new CustomSaga<IMessageInterface, MySagaData>
            {
                HandlerAction = (request, context, data) =>
                {
                    handlerInvoked = true;
                    return Task.FromResult(0);
                }
            };

            Test.Saga(saga)
                .WhenHandling<InterfaceImplementingMessage>();

            Assert.IsTrue(handlerInvoked);
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

    public class CustomSaga<TMessage, TSagaData> : NServiceBus.Saga<TSagaData>, IHandleMessages<TMessage> where TSagaData : class, IContainSagaData, new()
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

    public class MessageHierarchySaga :
        NServiceBus.Saga<MySagaData>,
        IAmStartedByMessages<BaseClassMessage>,
        IAmStartedByMessages<BaseClassImplementingMessage>
    {
        public bool BaseClassMessageHandlerInvoked { get; private set; }
        public bool BaseClassImplementingMessageHandlerInvoked { get; private set; }

        public Task Handle(BaseClassMessage message, IMessageHandlerContext context)
        {
            BaseClassMessageHandlerInvoked = true;

            return Task.FromResult(0);
        }

        public Task Handle(BaseClassImplementingMessage message, IMessageHandlerContext context)
        {
            BaseClassImplementingMessageHandlerInvoked = true;

            return Task.FromResult(0);
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<MySagaData> mapper)
        {
        }
    }

    public class BaseClassMessage : IMessage
    {
    }

    public class BaseClassImplementingMessage : BaseClassMessage
    {
    }

    public interface IMessageInterface : IMessage
    {
    }

    public class InterfaceImplementingMessage : IMessageInterface
    {
    }
}