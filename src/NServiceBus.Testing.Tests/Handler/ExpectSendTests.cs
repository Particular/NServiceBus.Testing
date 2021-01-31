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
            Test.Handler<SendingHandler<ISend1>>()
                .ExpectSend<ISend1>()
                .OnMessage<ITestMessage>();
        }

        [Test]
        public void ShouldPassExpectSendWithCheckIfSending()
        {
            Test.Handler<SendingHandler<ISend1>>()
                .ExpectSend<ISend1>(m => true)
                .OnMessage<ITestMessage>();
        }

        [Test]
        public void ShouldFailExpectSendIfNotSending()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<EmptyHandler>()
                .ExpectSend<ISend1>()
                .OnMessage<ITestMessage>());
        }

        [Test]
        public void ShouldFailExpectSendWithCheckIfNotSending()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<EmptyHandler>()
                .ExpectSend<ISend1>(m => true)
                .OnMessage<ITestMessage>());
        }

        [Test]
        public void ShouldFailExpectSendIfSendingWithoutMatch()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<SendingHandler<IPublish1>>()
                .ExpectSend<ISend1>(m => true)
                .OnMessage<ITestMessage>());
        }

        [Test]
        public void ShouldPassExpectSendProvidedSendOptionsToCheck()
        {
            var options = new SendOptions();
            SendOptions capturedOptions = null;

            Test.Handler<SendingHandler<ISend1>>()
                .WithExternalDependencies(handler => handler.OptionsProvider = () => options)
                .ExpectSend<ISend1>((message, sendOptions) =>
                {
                    capturedOptions = sendOptions;
                    return true;
                })
                .OnMessage<ITestMessage>();

            Assert.AreSame(options, capturedOptions);
        }

        [Test]
        public void ShouldPassExpectNotSendIfNotSending()
        {
            Test.Handler<EmptyHandler>()
                .ExpectNotSend<ISend1>()
                .OnMessage<ITestMessage>();
        }

        [Test]
        public void ShouldPassExpectNotSendWithCheckIfNotSending()
        {
            Test.Handler<EmptyHandler>()
                .ExpectNotSend<ISend1>(m => true)
                .OnMessage<ITestMessage>();
        }

        [Test]
        public void ShouldFailExpectNotSendIfSending()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<SendingHandler<ISend1>>()
                .ExpectNotSend<ISend1>()
                .OnMessage<ITestMessage>());
        }

        [Test]
        public void ShouldFailExpectNotSendWithCheckIfSending()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<SendingHandler<ISend1>>()
                .ExpectNotSend<ISend1>(m => true)
                .OnMessage<ITestMessage>());
        }

        [Test]
        public void ShouldPassExpectNotSentProvidedSendOptionsToCheck()
        {
            var options = new SendOptions();
            SendOptions capturedOptions = null;

            Test.Handler<SendingHandler<ISend1>>()
                .WithExternalDependencies(handler => handler.OptionsProvider = () => options)
                .ExpectNotSend<ISend1>((message, sendOptions) =>
                {
                    capturedOptions = sendOptions;
                    return false;
                })
                .OnMessage<ITestMessage>();

            Assert.AreSame(options, capturedOptions);
        }

        [Test]
        public void ShouldFailExpectSendLocalIfNotSendingLocal()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<SendingHandler<ISend1>>()
                .ExpectSendLocal<ISend1>()
                .OnMessage<ITestMessage>());
        }

        [Test]
        public void ShouldFailExpectSendLocalWithCheckIfNotSendingLocal()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<SendingHandler<ISend1>>()
                .ExpectSendLocal<ISend1>(m => true)
                .OnMessage<ITestMessage>());
        }

        [Test]
        public void ShouldPassExpectSendLocalIfSendingLocal()
        {
            Test.Handler<SendingLocalHandler<ISend1>>()
                .ExpectSendLocal<ISend1>()
                .OnMessage<ITestMessage>();
        }

        [Test]
        public void ShouldPassExpectSendLocalWithCheckIfSendingLocal()
        {
            Test.Handler<SendingLocalHandler<ISend1>>()
                .ExpectSendLocal<ISend1>(m => true)
                .OnMessage<ITestMessage>();
        }

        [Test]
        public void ShouldFailExpectSendLocalIfSendingLocalWithoutMatch()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<SendingLocalHandler<IPublish1>>()
                .ExpectSendLocal<ISend1>(m => true)
                .OnMessage<ITestMessage>());
        }

        [Test]
        public void ShouldFailExpectSendLocalIfSendingLocalWithFailingCheck()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<SendingLocalHandler<ISend1>>()
                .ExpectSendLocal<ISend1>(m => false)
                .OnMessage<ITestMessage>());
        }

        [Test]
        public void ShouldPassExpectNotSendLocalIfNotSendingLocal()
        {
            Test.Handler<EmptyHandler>()
                .ExpectNotSendLocal<ISend1>(m => true)
                .OnMessage<ITestMessage>();
        }

        [Test]
        public void ShouldFailExpectNotSendLocalIfSendingLocal()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<SendingLocalHandler<ISend1>>()
                .ExpectNotSendLocal<ISend1>(m => true)
                .OnMessage<ITestMessage>());
        }

        [Test]
        public void ShouldPassExpectNotSendLocalIfSendingLocalWithoutMatch()
        {
            Test.Handler<SendingLocalHandler<IPublish1>>()
                .ExpectNotSendLocal<ISend1>(m => true)
                .OnMessage<ITestMessage>();
        }

        [Test]
        public void ShouldPassExpectNotSendLocalIfSendingLocalWithFailingCheck()
        {
            Test.Handler<SendingLocalHandler<ISend1>>()
                .ExpectNotSendLocal<ISend1>(m => false)
                .OnMessage<ITestMessage>();
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
                    h.HandlerAction = context => context.Send<ISend1>(m => { });
                })
                .ExpectSend<ISend1>(m =>
                {
                    Interlocked.Increment(ref counter);
                    return false;
                })
                .OnMessage<MyCommand>());
            Assert.AreEqual(100, counter);
        }

        public class SendingHandler<TSend> : IHandleMessages<ITestMessage>
        where TSend : IMessage
        {
            public Action<TSend> ModifyMessage { get; set; } = m => { };

            public Func<SendOptions> OptionsProvider { get; set; } = () => new SendOptions();

            public Task Handle(ITestMessage message, IMessageHandlerContext context)
            {
                return context.Send(ModifyMessage, OptionsProvider());
            }
        }

        public class SendingLocalHandler<TSend> : IHandleMessages<ITestMessage>
        where TSend : IMessage
        {
            public Action<TSend> ModifyMessage { get; set; } = m => { };

            public Task Handle(ITestMessage message, IMessageHandlerContext context)
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