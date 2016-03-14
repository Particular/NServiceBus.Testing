namespace NServiceBus.Testing.Tests.Handler
{
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class ExpectHandleCurrentMessageLaterTests
    {
        [Test]
        public void ShouldAssertHandleCurrentMessageLaterWasCalled()
        {
            Test.Handler<HandleCurrentMessageLaterHandler>()
                .ExpectHandleCurrentMessageLater()
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldFailAssertingHandleCurrentMessageLaterWasCalled()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<EmptyHandler>()
                .ExpectHandleCurrentMessageLater()
                .OnMessage<TestMessage>());
        }

        public class HandleCurrentMessageLaterHandler : IHandleMessages<TestMessage>
        {
            public Task Handle(TestMessage message, IMessageHandlerContext context)
            {
                return context.HandleCurrentMessageLater();
            }
        }
    }
}