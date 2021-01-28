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
                .OnMessage<ITestMessage>();
        }

        [Test]
        public void ShouldFailAssertingDoNotContinueDispatchingCurrentMessageToHandlersWasCalled()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<EmptyHandler>()
                .ExpectDoNotContinueDispatchingCurrentMessageToHandlers()
                .OnMessage<ITestMessage>());
        }

        public class DoNotContinueDispatchingCurrentMessageToHandlersHandler : IHandleMessages<ITestMessage>
        {
            public Task Handle(ITestMessage message, IMessageHandlerContext context)
            {
                context.DoNotContinueDispatchingCurrentMessageToHandlers();

                return Task.FromResult(0);
            }
        }
    }
}