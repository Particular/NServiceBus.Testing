namespace NServiceBus.Testing.Tests
{
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class Issue_933
    {
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

        public class MyCommand : ICommand
        {
            public string Header1 { get; set; }
            public string Header2 { get; set; }
        }

        public class MyCommandHandler : IHandleMessages<MyCommand>
        {
            public Task Handle(MyCommand message, IMessageHandlerContext context)
            {
                message.Header1 = context.MessageHeaders["Key1"];
                message.Header2 = context.MessageHeaders["Key2"];

                return Task.FromResult(0);
            }
        }
    }
}
