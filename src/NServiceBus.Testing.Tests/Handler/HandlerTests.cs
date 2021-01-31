namespace NServiceBus.Testing.Tests.Handler
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading.Tasks;
    using NUnit.Framework;
    using Saga;

    [TestFixture]
    public class HandlerTests
    {
        [Test]
        public void ShouldInvokeMessageInitializerOnMessage()
        {
            Test.Handler<ReplyingHandler>()
                .ExpectReply<MyReply>(m => m.String == "hello")
                .OnMessage<MyRequest>(m =>
                {
                    m.String = "hello";
                    m.ShouldReply = true;
                });
        }

        [Test]
        public void ShouldCallHandleOnExplicitInterfaceImplementation()
        {
            var handler = new ExplicitInterfaceImplementation();
            Assert.IsFalse(handler.IsHandled);
            Test.Handler(handler).OnMessage<ITestMessage>();
            Assert.IsTrue(handler.IsHandled);
        }

        [Test]
        public void Should_be_able_to_pass_an_already_constructed_message_into_handler_without_specifying_id()
        {
            const string expected = "dummy";

            var handler = new TestMessageWithPropertiesHandler();
            var message = new TestMessageWithProperties
            {
                Dummy = expected
            };
            Test.Handler(handler)
                .OnMessage(message);
            Assert.AreEqual(expected, handler.ReceivedDummyValue);
            Assert.DoesNotThrow(() => Guid.Parse(handler.AssignedMessageId), "Message ID should be a valid GUID.");
        }

        [Test]
        public void SendMessageWithMultiIncomingHeaders()
        {
            var command = new MyCommand();

            Test.Handler<MyCommandHandler>()
                .SetIncomingHeader("Key1", "Header1")
                .SetIncomingHeader("Key2", "Header2")
                .OnMessage(command);

            Assert.AreEqual("Header1", command.Header1);
            Assert.AreEqual("Header2", command.Header2);
        }

        [Test]
        public void ShouldBeAbleToConfigureMessageHandlerContext()
        {
            var messageId = Guid.NewGuid().ToString();
            var replyToAddress = "0118 999 881 999 119 725 3";
            var handler = new ContextAccessingHandler();
            TestableMessageHandlerContext contextInstance = null;

            Test.Handler(handler)
                .ConfigureHandlerContext(c =>
                {
                    c.MessageId = messageId;
                    c.ReplyToAddress = replyToAddress;
                    contextInstance = c;
                })
                .OnMessage<ITestMessage>();

            Assert.AreEqual(messageId, handler.Context.MessageId);
            Assert.AreEqual(replyToAddress, handler.Context.ReplyToAddress);
            Assert.AreSame(contextInstance, handler.Context);
        }

        [Test]
        public void OnMessageShouldAwaitAsyncTasks()
        {
            Test.Handler<AsyncHandler>()
                .ExpectSend<ISend1>(m => true)
                .OnMessage<MyCommand>();
        }

        [Test]
        public void ShouldHandleInterfaceImplementingMessages()
        {
            var handler = new InterfaceMessageHandler();
            Test.Handler(handler)
                .OnMessage(new InterfaceImplementingMessage());

            Assert.IsTrue(handler.HandlerInvoked);
        }

        public void ShouldHandleBaseClassImplementingMessages()
        {
            var handler = new InterfaceMessageHandler();
            Test.Handler(handler)
                .OnMessage(new BaseClassImplementingMessage());

            Assert.IsTrue(handler.HandlerInvoked);
        }

        [Test]
        public void ShouldInvokeAllHandlerMethodsWhenHandlingSubclassedMessage()
        {
            var handler = new MessageHierarchyHandler();
            Test.Handler(handler)
                .OnMessage(new BaseClassImplementingMessage());

            Assert.IsTrue(handler.BaseClassMessageHandlerInvoked);
            Assert.IsTrue(handler.BaseClassImplementingMessageHandlerInvoked);
        }

        [Test]
        public void ShouldOnlyInvokeBaseClassHandlerMethofWhenHandlingBaseClassMessage()
        {
            var handler = new MessageHierarchyHandler();
            Test.Handler(handler)
                .OnMessage(new BaseClassMessage());

            Assert.IsTrue(handler.BaseClassMessageHandlerInvoked);
            Assert.IsFalse(handler.BaseClassImplementingMessageHandlerInvoked);
        }
    }

    class MyCommand : ICommand
    {
        public string Header1 { get; set; }
        public string Header2 { get; set; }
    }

    public class ContextAccessingHandler : IHandleMessages<ITestMessage>
    {
        public IMessageHandlerContext Context { get; private set; }

        public Task Handle(ITestMessage message, IMessageHandlerContext context)
        {
            Context = context;
            return Task.FromResult(0);
        }
    }

    public class EmptyHandler : IHandleMessages<ITestMessage>
    {
        public Task Handle(ITestMessage message, IMessageHandlerContext context)
        {
            return Task.FromResult(0);
        }
    }

    class ConcurrentHandler : IHandleMessages<MyCommand>
    {
        public int NumberOfThreads { get; set; } = 100;

        public Task Handle(MyCommand message, IMessageHandlerContext context)
        {
            var operations = new ConcurrentBag<Task>();

            Parallel.For(0, NumberOfThreads, x => operations.Add(HandlerAction(context)));

            return Task.WhenAll(operations);
        }

        public Func<IMessageHandlerContext, Task> HandlerAction = x => Task.FromResult(0);
    }

    public interface ITestMessage : IMessage
    {
    }

    public interface IPublish1 : IMessage
    {
        string Data { get; set; }
    }

    public interface ISend1 : IMessage
    {
        string Data { get; set; }
    }

    public interface IPublish2 : IMessage
    {
        string Data { get; set; }
    }

    public class Outgoing : IMessage
    {
        public int Number { get; set; }
    }

    public class Outgoing2 : IMessage
    {
        public int Number { get; set; }
    }

    public class Incoming : IMessage
    {
    }

    public class ReplyingHandler : IHandleMessages<MyRequest>
    {
        public Func<ReplyOptions> OptionsProvider { get; set; } = () => new ReplyOptions();

        public Task Handle(MyRequest message, IMessageHandlerContext context)
        {
            return message.ShouldReply ? context.Reply(new MyReply
            {
                String = message.String
            }, OptionsProvider()) : Task.FromResult(0);
        }
    }

    public class TestMessageWithPropertiesHandler : IHandleMessages<TestMessageWithProperties>
    {
        public Task Handle(TestMessageWithProperties message, IMessageHandlerContext context)
        {
            ReceivedDummyValue = message.Dummy;
            AssignedMessageId = context.MessageId;
            return Task.FromResult(0);
        }

        public string ReceivedDummyValue;
        public string AssignedMessageId;
    }

    public class TestMessageWithProperties : IMessage
    {
        public string Dummy { get; set; }
    }

    class MyCommandHandler : IHandleMessages<MyCommand>
    {
        public Task Handle(MyCommand message, IMessageHandlerContext context)
        {
            message.Header1 = context.MessageHeaders["Key1"];
            message.Header2 = context.MessageHeaders["Key2"];

            return Task.FromResult(0);
        }
    }

    class AsyncHandler : IHandleMessages<MyCommand>
    {
        public async Task Handle(MyCommand message, IMessageHandlerContext context)
        {
            await Task.Yield();
            await context.Send<ISend1>(m => { }, new SendOptions());
        }
    }

    public class ExplicitInterfaceImplementation : IHandleMessages<ITestMessage>
    {
        public bool IsHandled { get; set; }

        Task IHandleMessages<ITestMessage>.Handle(ITestMessage message, IMessageHandlerContext context)
        {
            IsHandled = true;

            return Task.FromResult(0);
        }
    }

    public class InterfaceMessageHandler : IHandleMessages<IMessageInterface>
    {
        public bool HandlerInvoked { get; private set; }

        public Task Handle(IMessageInterface message, IMessageHandlerContext context)
        {
            HandlerInvoked = true;

            return Task.FromResult(0);
        }
    }

    public interface IMessageInterface : IMessage
    {
    }

    public class InterfaceImplementingMessage : IMessageInterface
    {
    }

    public class BaseClassMessageHandler : IHandleMessages<BaseClassMessage>
    {
        public bool HandlerInvoked { get; private set; }

        public Task Handle(BaseClassMessage message, IMessageHandlerContext context)
        {
            HandlerInvoked = true;

            return Task.FromResult(0);
        }
    }

    public class MessageHierarchyHandler :
        IHandleMessages<BaseClassMessage>,
        IHandleMessages<BaseClassImplementingMessage>
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
    }

    public class BaseClassMessage : IMessage
    {
    }

    public class BaseClassImplementingMessage : BaseClassMessage
    {
    }
}