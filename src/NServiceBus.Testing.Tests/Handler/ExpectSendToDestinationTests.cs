namespace NServiceBus.Testing.Tests.Handler
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class ExpectSendToDestinationTests
    {
        [Test]
        public void ShouldPassWhenDestinationMatches()
        {
            var expectedDestination = "expected destination";

            Test.Handler<SendingHandler<Send1>>()
                .WithExternalDependencies(h => h.OptionsProvider = () =>
                {
                    var options = new SendOptions();
                    options.SetDestination(expectedDestination);
                    return options;
                })
                .ExpectSendToDestination<Send1>((message, destination) => destination == expectedDestination)
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldNotPassWhenDestinationDoesNotMatch()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<SendingHandler<TestMessage>>()
                .WithExternalDependencies(h => h.OptionsProvider = () =>
                {
                    var options = new SendOptions();
                    options.SetDestination("somewhere");
                    return options;
                })
                .ExpectSendToDestination<Send1>((message, destination) => destination == "anywhere")
                .OnMessage<TestMessage>());
        }

        class SendingHandler<TSend> : IHandleMessages<TestMessage> where TSend : IMessage
        {
            public Action<TSend> ModifyMessage { get; set; } = m => { };

            public Func<SendOptions> OptionsProvider { get; set; } = () => new SendOptions();

            public Task Handle(TestMessage message, IMessageHandlerContext context)
            {
                return context.Send(ModifyMessage, OptionsProvider());
            }
        }
    }
}