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
            Test.Handler<SendingHandler<ISend1>>()
                .WithExternalDependencies(h => h.OptionsProvider = () =>
                {
                    var options = new SendOptions();
                    options.SetDestination("somewhere");
                    return options;
                })
                .ExpectNotSendToDestination<ISend1>((message, destination) => destination == "anywhere")
                .OnMessage<ITestMessage>();
        }

        [Test]
        public void ShouldNotPassWhenDestinationDoesMatch()
        {
            var expectedDestination = "expected destination";

            Assert.Throws<ExpectationException>(() => Test.Handler<SendingHandler<ISend1>>()
                .WithExternalDependencies(h => h.OptionsProvider = () =>
                {
                    var options = new SendOptions();
                    options.SetDestination(expectedDestination);
                    return options;
                })
                .ExpectNotSendToDestination<ISend1>((message, destination) => destination == expectedDestination)
                .OnMessage<ITestMessage>());
        }

        [Test]
        public void ShouldPassWhenNoMessageIsSent()
        {
            Test.Handler<SendingHandler<ITestMessage>>()
                .WithExternalDependencies(h => h.SendAnything = false)
                .ExpectNotSendToDestination<ISend1>()
                .OnMessage<ITestMessage>();
        }

        class SendingHandler<TSend> : IHandleMessages<ITestMessage> where TSend : IMessage
        {
            public Action<TSend> ModifyMessage { get; set; } = m => { };

            public Func<SendOptions> OptionsProvider { get; set; } = () => new SendOptions();

            public bool SendAnything { get; set; } = true;

            public Task Handle(ITestMessage message, IMessageHandlerContext context)
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