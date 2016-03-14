namespace NServiceBus.Testing.Tests.Handler
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class ExpectSendTests
    {
        [Test]
        public void ShouldPassExpectSendIfSending()
        {
            Test.Handler<SendingHandler<Send1>>()
                .ExpectSend<Send1>()
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldPassExpectSendWithCheckIfSending()
        {
            Test.Handler<SendingHandler<Send1>>()
                .ExpectSend<Send1>(m => true)
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldFailExpectSendIfNotSending()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<EmptyHandler>()
                .ExpectSend<Send1>()
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldFailExpectSendWithCheckIfNotSending()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<EmptyHandler>()
                .ExpectSend<Send1>(m => true)
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldFailExpectSendIfSendingWithoutMatch()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<SendingHandler<Publish1>>()
                .ExpectSend<Send1>(m => true)
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldPassExpectSendProvidedSendOptionsToCheck()
        {
            var options = new SendOptions();
            SendOptions capturedOptions = null;

            Test.Handler<SendingHandler<Send1>>()
                .WithExternalDependencies(handler => handler.OptionsProvider = () => options)
                .ExpectSend<Send1>((message, sendOptions) =>
                {
                    capturedOptions = sendOptions;
                    return true;
                })
                .OnMessage<TestMessage>();

            Assert.AreSame(options, capturedOptions);
        }

        [Test]
        public void ShouldPassExpectNotSendIfNotSending()
        {
            Test.Handler<EmptyHandler>()
                .ExpectNotSend<Send1>()
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldPassExpectNotSendWithCheckIfNotSending()
        {
            Test.Handler<EmptyHandler>()
                .ExpectNotSend<Send1>(m => true)
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldFailExpectNotSendIfSending()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<SendingHandler<Send1>>()
                .ExpectNotSend<Send1>()
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldFailExpectNotSendWithCheckIfSending()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<SendingHandler<Send1>>()
                .ExpectNotSend<Send1>(m => true)
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldPassExpectNotSentProvidedSendOptionsToCheck()
        {
            var options = new SendOptions();
            SendOptions capturedOptions = null;

            Test.Handler<SendingHandler<Send1>>()
                .WithExternalDependencies(handler => handler.OptionsProvider = () => options)
                .ExpectNotSend<Send1>((message, sendOptions) =>
                {
                    capturedOptions = sendOptions;
                    return false;
                })
                .OnMessage<TestMessage>();

            Assert.AreSame(options, capturedOptions);
        }

        [Test]
        public void ShouldFailExpectSendLocalIfNotSendingLocal()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<SendingHandler<Send1>>()
                .ExpectSendLocal<Send1>()
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldFailExpectSendLocalWithCheckIfNotSendingLocal()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<SendingHandler<Send1>>()
                .ExpectSendLocal<Send1>(m => true)
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldPassExpectSendLocalIfSendingLocal()
        {
            Test.Handler<SendingLocalHandler<Send1>>()
                .ExpectSendLocal<Send1>()
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldPassExpectSendLocalWithCheckIfSendingLocal()
        {
            Test.Handler<SendingLocalHandler<Send1>>()
                .ExpectSendLocal<Send1>(m => true)
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldFailExpectSendLocalIfSendingLocalWithoutMatch()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<SendingLocalHandler<Publish1>>()
                .ExpectSendLocal<Send1>(m => true)
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldFailExpectSendLocalIfSendingLocalWithFailingCheck()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<SendingLocalHandler<Send1>>()
                .ExpectSendLocal<Send1>(m => false)
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldPassExpectNotSendLocalIfNotSendingLocal()
        {
            Test.Handler<EmptyHandler>()
                .ExpectNotSendLocal<Send1>(m => true)
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldFailExpectNotSendLocalIfSendingLocal()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<SendingLocalHandler<Send1>>()
                .ExpectNotSendLocal<Send1>(m => true)
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldPassExpectNotSendLocalIfSendingLocalWithoutMatch()
        {
            Test.Handler<SendingLocalHandler<Publish1>>()
                .ExpectNotSendLocal<Send1>(m => true)
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldPassExpectNotSendLocalIfSendingLocalWithFailingCheck()
        {
            Test.Handler<SendingLocalHandler<Send1>>()
                .ExpectNotSendLocal<Send1>(m => false)
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldSupportSendingDifferentMessagesAtOnce()
        {
            var result = 0;

            Test.Handler<SendingManyWithDifferentMessagesHandler>()
                .ExpectSend<Outgoing>(m =>
                {
                    result += m.Number;
                    return true;
                })
                .ExpectSend<Outgoing2>(m =>
                {
                    result += m.Number;
                    return true;
                })
                .OnMessage<Incoming>();

            Assert.AreEqual(3, result);
        }

        [Test]
        public void ShouldSupportSendingManyMessagesAtOnce()
        {
            Test.Handler<SendingManyHandler>()
                .ExpectSend<Outgoing>(m => m.Number == 1)
                .OnMessage<Incoming>();

            Test.Handler<SendingManyHandler>()
                .ExpectSend<Outgoing>(m => m.Number == 2)
                .OnMessage<Incoming>();
        }

        [Test]
        public void SendShouldBeThreadsafe()
        {
            var counter = 0;

            Assert.Throws<ExpectationException>(() => Test.Handler<ConcurrentHandler>()
                .WithExternalDependencies(h =>
                {
                    h.NumberOfThreads = 100;
                    h.HandlerAction = context => context.Send<Send1>(m => { });
                })
                .ExpectSend<Send1>(m =>
                {
                    Interlocked.Increment(ref counter);
                    return false;
                })
                .OnMessage<MyCommand>());
            Assert.AreEqual(100, counter);
        }

        public class SendingHandler<TSend> : IHandleMessages<TestMessage>
        where TSend : IMessage
        {
            public Action<TSend> ModifyMessage { get; set; } = m => { };

            public Func<SendOptions> OptionsProvider { get; set; } = () => new SendOptions();

            public Task Handle(TestMessage message, IMessageHandlerContext context)
            {
                return context.Send(ModifyMessage, OptionsProvider());
            }
        }

        public class SendingLocalHandler<TSend> : IHandleMessages<TestMessage>
        where TSend : IMessage
        {
            public Action<TSend> ModifyMessage { get; set; } = m => { };

            public Task Handle(TestMessage message, IMessageHandlerContext context)
            {
                return context.SendLocal(ModifyMessage);
            }
        }

        public class SendingManyWithDifferentMessagesHandler : IHandleMessages<Incoming>
        {
            public async Task Handle(Incoming message, IMessageHandlerContext context)
            {
                await context.Send<Outgoing>(m => { m.Number = 1; });

                await context.Send<Outgoing2>(m => { m.Number = 2; });
            }
        }

        public class SendingManyHandler : IHandleMessages<Incoming>
        {
            public async Task Handle(Incoming message, IMessageHandlerContext context)
            {
                await context.Send<Outgoing>(m => { m.Number = 1; });

                await context.Send<Outgoing>(m => { m.Number = 2; });
            }
        }
    }
}