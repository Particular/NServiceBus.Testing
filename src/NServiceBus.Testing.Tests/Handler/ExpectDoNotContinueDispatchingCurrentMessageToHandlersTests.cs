namespace NServiceBus.Testing.Tests.Handler
{
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class ExpectDoNotContinueDispatchingCurrentMessageToHandlersTests
    {
        [Test]
        public void ShouldAssertDoNotContinueDispatchingCurrentMessageToHandlersWasCalled()
        {
            Test.Handler<DoNotContinueDispatchingCurrentMessageToHandlersHandler>()
                .ExpectDoNotContinueDispatchingCurrentMessageToHandlers()
                .OnMessage<TestMessage>();
        }

        [Test]
        public void ShouldFailAssertingDoNotContinueDispatchingCurrentMessageToHandlersWasCalled()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<EmptyHandler>()
                .ExpectDoNotContinueDispatchingCurrentMessageToHandlers()
                .OnMessage<TestMessage>());
        }

        public class DoNotContinueDispatchingCurrentMessageToHandlersHandler : IHandleMessages<TestMessage>
        {
            public Task Handle(TestMessage message, IMessageHandlerContext context)
            {
                context.DoNotContinueDispatchingCurrentMessageToHandlers();

                return Task.FromResult(0);
            }
        }
    }
}