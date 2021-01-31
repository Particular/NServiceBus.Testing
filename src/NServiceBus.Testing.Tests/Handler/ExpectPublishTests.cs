namespace NServiceBus.Testing.Tests.Handler
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class ExpectPublishTests
    {
        [Test]
        public void ShouldPassExpectPublishWhenPublishing()
        {
            Test.Handler<PublishingHandler<IPublish1>>()
                .ExpectPublish<IPublish1>()
                .OnMessage<ITestMessage>();
        }

        [Test]
        public void ShouldPassExpectPublishWhenPublishingWithCustomCheck()
        {
            Test.Handler<PublishingHandler<IPublish1>>()
                .ExpectPublish<IPublish1>(m => true)
                .OnMessage<ITestMessage>();
        }

        [Test]
        public void ShouldFailExpectNotPublishWhenPublishing()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<PublishingHandler<IPublish1>>()
                .ExpectNotPublish<IPublish1>()
                .OnMessage<ITestMessage>());
        }

        [Test]
        public void ShouldFailExpectNotPublishWithCheckWhenPublishing()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<PublishingHandler<IPublish1>>()
                .ExpectNotPublish<IPublish1>(m => true)
                .OnMessage<ITestMessage>());
        }

        [Test]
        public void ShouldPassExpectPublishWhenPublishingMultipleEvents()
        {
            Test.Handler<PublishingHandler<IPublish1, IPublish2>>()
                .ExpectPublish<IPublish1>(m => true)
                .OnMessage<ITestMessage>();
        }

        [Test]
        public void ShouldPassExpectPublishWhenMessageIsSend()
        {
            Test.Handler<PublishingHandler<IPublish1>>()
                .ExpectPublish<IPublish1>(m => true)
                .OnMessage(new TestMessageImpl(), Guid.NewGuid().ToString());
        }

        [Test]
        public void ShouldPassExpectPublishWhenPublishingAndCheckingPredicate()
        {
            Test.Handler<PublishingHandler<IPublish1>>()
                .WithExternalDependencies(h => h.ModifyMessage = m => m.Data = "Data")
                .ExpectPublish<IPublish1>(m => m.Data == "Data")
                .OnMessage<ITestMessage>();
        }

        [Test]
        public void ShouldFailExpectNotPublishWhenPublishingAndCheckingPredicate()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<PublishingHandler<IPublish1>>()
                .WithExternalDependencies(h => h.ModifyMessage = m => m.Data = "Data")
                .ExpectNotPublish<IPublish1>(m => m.Data == "Data")
                .OnMessage<ITestMessage>());
        }

        [Test]
        public void ShouldFailExpectPublishWhenPublishingAndCheckingPredicateThatFails()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<PublishingHandler<IPublish1>>()
                .WithExternalDependencies(h => h.ModifyMessage = m => m.Data = "NotData")
                .ExpectPublish<IPublish1>(m => m.Data == "Data")
                .OnMessage<ITestMessage>());
        }

        [Test]
        public void ShouldPassExpectNotPublishWhenPublishingAndCheckingPredicateThatFails()
        {
            Test.Handler<PublishingHandler<IPublish1>>()
                .WithExternalDependencies(h => h.ModifyMessage = m => m.Data = "NotData")
                .ExpectNotPublish<IPublish1>(m => m.Data == "Data")
                .OnMessage<ITestMessage>();
        }

        [Test]
        public void ShouldFailExpectPublishIfNotPublishing()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<EmptyHandler>()
                .ExpectPublish<IPublish1>(m => true)
                .OnMessage<ITestMessage>());
        }

        [Test]
        public void ShouldPassExpectPublishProvidedPublishOptionsToCheck()
        {
            var options = new PublishOptions();
            PublishOptions capturedOptions = null;

            Test.Handler<PublishingHandler<IPublish1>>()
                .WithExternalDependencies(h => h.OptionsProvider = () => options)
                .ExpectPublish<IPublish1>((message, publishOptions) =>
                {
                    capturedOptions = publishOptions;
                    return true;
                })
                .OnMessage<ITestMessage>();

            Assert.AreSame(options, capturedOptions);
        }

        [Test]
        public void ShouldPassExpectNotPublishIfNotPublishing()
        {
            Test.Handler<EmptyHandler>()
                .ExpectNotPublish<IPublish1>()
                .OnMessage<ITestMessage>();
        }

        [Test]
        public void ShouldPassExpectNotPublishWithCheckIfNotPublishing()
        {
            Test.Handler<EmptyHandler>()
                .ExpectNotPublish<IPublish1>(m => true)
                .OnMessage<ITestMessage>();
        }

        [Test]
        public void ShouldPassExpectNotPublishProvidedPublishOptionsToCheck()
        {
            var options = new PublishOptions();
            PublishOptions capturedOptions = null;

            Test.Handler<PublishingHandler<IPublish1>>()
                .WithExternalDependencies(h => h.OptionsProvider = () => options)
                .ExpectNotPublish<IPublish1>((message, publishOptions) =>
                {
                    capturedOptions = publishOptions;
                    return false;
                })
                .OnMessage<ITestMessage>();

            Assert.AreSame(options, capturedOptions);
        }

        [Test]
        public void ShouldFailExpectPublishIfPublishWrongMessageType()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<PublishingHandler<IPublish1>>()
                .ExpectPublish<IPublish2>(m => true)
                .OnMessage<ITestMessage>());
        }

        [Test]
        public void ShouldPassExpectNotPublishIfPublishWrongMessageType()
        {
            Test.Handler<PublishingHandler<IPublish1>>()
                .ExpectNotPublish<IPublish2>(m => true)
                .OnMessage<ITestMessage>();
        }

        [Test]
        public void ShouldSupportDataBusProperties()
        {
            Test.Handler<DataBusMessageHandler>()
                .ExpectNotPublish<IPublish2>(m => true)
                .OnMessage<MessageWithDataBusProperty>();
        }

        [Test]
        public void ShouldSupportPublishMoreThanOneMessageAtOnce()
        {
            Test.Handler<PublishingManyHandler>()
                .ExpectPublish<Outgoing>(m => true)
                .ExpectPublish<Outgoing>(m => true)
                .OnMessage<Incoming>();
        }

        [Test]
        public void PublishShouldBeThreadsafe()
        {
            var counter = 0;

            Assert.Throws<ExpectationException>(() => Test.Handler<ConcurrentHandler>()
                .WithExternalDependencies(h =>
                {
                    h.NumberOfThreads = 100;
                    h.HandlerAction = context => context.Publish<IPublish1>(m => { });
                })
                .ExpectPublish<IPublish1>(m =>
                {
                    Interlocked.Increment(ref counter);
                    return false;
                })
                .OnMessage<MyCommand>());

            Assert.AreEqual(100, counter);
        }

        public class PublishingHandler<TPublish> : IHandleMessages<ITestMessage>
        where TPublish : IMessage
        {
            public Action<TPublish> ModifyMessage { get; set; } = m => { };

            public Func<PublishOptions> OptionsProvider { get; set; } = () => new PublishOptions();

            public Task Handle(ITestMessage message, IMessageHandlerContext context)
            {
                return context.Publish(ModifyMessage, OptionsProvider());
            }
        }

        public class PublishingHandler<TPublish1, TPublish2> : IHandleMessages<ITestMessage>
        where TPublish1 : IMessage
        where TPublish2 : IMessage
        {
            public Action<TPublish1> ModifyPublish1 { get; set; } = m => { };
            public Action<TPublish2> ModifyPublish2 { get; set; } = m => { };

            public async Task Handle(ITestMessage message, IMessageHandlerContext context)
            {
                await context.Publish(ModifyPublish1);
                await context.Publish(ModifyPublish2);
            }
        }

        class TestMessageImpl : ITestMessage
        {
        }

        public class DataBusMessageHandler : IHandleMessages<MessageWithDataBusProperty>
        {
            public Task Handle(MessageWithDataBusProperty message, IMessageHandlerContext context)
            {
                return Task.FromResult(0);
            }
        }

        public class MessageWithDataBusProperty : IMessage
        {
        }

        public class PublishingManyHandler : IHandleMessages<Incoming>
        {
            public async Task Handle(Incoming message, IMessageHandlerContext context)
            {
                await context.Publish<Outgoing>(m => { m.Number = 1; });

                await context.Publish<Outgoing>(m => { m.Number = 2; });
            }
        }
    }
}