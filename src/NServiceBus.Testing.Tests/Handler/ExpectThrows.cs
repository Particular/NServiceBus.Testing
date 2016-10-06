namespace NServiceBus.Testing.Tests.Handler
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class ExpectThrows
    {
        [Test]
        public void OnMessageShouldThrowExpectedExceptionWhenExceptionExpectedButNotThrown()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<NotThrowingHandler>().ExpectThrows<InvalidOperationException>().OnMessage<TestMessage>());
        }

        [Test]
        public void OnMessageShouldNotThrowInnerExceptionWhenSpecificExceptionExpected()
        {
            Assert.DoesNotThrow(() => Test.Handler<ThrowingInvalidOperationExceptionHandler>().ExpectThrows<InvalidOperationException>().OnMessage<TestMessage>());
        }

        [Test]
        public void OnMessageShouldNotThrowInnerExceptionWhenExceptionExpected()
        {
            Assert.DoesNotThrow(() => Test.Handler<ThrowingInvalidOperationExceptionHandler>().ExpectThrows().OnMessage<TestMessage>());
        }

        [Test]
        public void OnMessageShouldNotThrowInnerExceptionWhenExceptionFulfillsCheck()
        {
            Assert.DoesNotThrow(() => Test.Handler<ThrowingInvalidOperationExceptionHandler>().ExpectThrows<InvalidOperationException>(e => e.InnerException == null).OnMessage<TestMessage>());
        }

        [Test]
        public void OnMessageShouldThrowExpectedExceptionWhenSpecificExceptionWasNotThrown()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<ThrowingArgumentExceptionHandler>().ExpectThrows<InvalidOperationException>().OnMessage<TestMessage>());
        }

        [Test]
        public void OnMessageShouldThrowExpectedExceptionWhenExceptionFulfillsCheck()
        {
            Assert.Throws<ExpectationException>(() => Test.Handler<ThrowingArgumentExceptionHandler>().ExpectThrows<InvalidOperationException>(e => e.InnerException == null).OnMessage<TestMessage>());
        }

        public class ThrowingInvalidOperationExceptionHandler : IHandleMessages<TestMessage>
        {
            public Task Handle(TestMessage message, IMessageHandlerContext context)
            {
                throw new InvalidOperationException();
            }
        }

        public class ThrowingArgumentExceptionHandler : IHandleMessages<TestMessage>
        {
            public Task Handle(TestMessage message, IMessageHandlerContext context)
            {
                throw new ArgumentException();
            }
        }

        public class NotThrowingHandler : IHandleMessages<TestMessage>
        {
            public Task Handle(TestMessage message, IMessageHandlerContext context)
            {
                return Task.FromResult(0);
            }
        }
    }
}