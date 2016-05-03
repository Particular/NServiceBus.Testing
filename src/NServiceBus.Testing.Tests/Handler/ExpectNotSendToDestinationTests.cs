namespace NServiceBus.Testing.Tests.Handler
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class ExpectNotSendToDestinationTests
    {
        [Test]
        public void ShouldPassWhenDestinationDoesNotMatch()
        {
            Test.Handler<SendingHandler<Send1>>()
                .WithExternalDependencies(h => h.OptionsProvider = () =>
                {
                    var options = new SendOptions();
                    options.SetDestination("somewhere");
                    return options;
                })
                .ExpectNotSendToDestination<Send1>((message, destination) => destination == "anywhere")
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldNotPassWhenDestinationDoesMatch()
        {
            var expectedDestination = "expected destination";

            Assert.Throws<ExpectationException>(() => Test.Handler<SendingHandler<Send1>>()
                .WithExternalDependencies(h => h.OptionsProvider = () =>
                {
                    var options = new SendOptions();
                    options.SetDestination(expectedDestination);
                    return options;
                })
                .ExpectNotSendToDestination<Send1>((message, destination) => destination == expectedDestination)
                .OnMessage<TestMessage>());
        }

        [Test]
        public void ShouldPassWhenNoMessageIsSent()
        {
            Test.Handler<SendingHandler<TestMessage>>()
                .WithExternalDependencies(h => h.SendAnything = false)
                .ExpectNotSendToDestination<Send1>((message, destination) => true)
                .OnMessage<TestMessage>();
        }

        class SendingHandler<TSend> : IHandleMessages<TestMessage> where TSend : IMessage
        {
            public Action<TSend> ModifyMessage { get; set; } = m => { };

            public Func<SendOptions> OptionsProvider { get; set; } = () => new SendOptions();

            public bool SendAnything { get; set; } = true;

            public Task Handle(TestMessage message, IMessageHandlerContext context)
            {
                if (SendAnything)
                {
                    return context.Send(ModifyMessage, OptionsProvider());
                }
                return Task.FromResult(0);
            }
        }
    }
}