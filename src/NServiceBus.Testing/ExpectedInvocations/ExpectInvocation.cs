namespace NServiceBus.Testing
{
    using System.Runtime.ExceptionServices;

    abstract class ExpectInvocation
    {
        public abstract void Validate(TestableMessageHandlerContext context, ExceptionDispatchInfo exceptionInfo = null);

        protected void Fail(string message)
        {
            throw new ExpectationException(message);
        }
    }
}