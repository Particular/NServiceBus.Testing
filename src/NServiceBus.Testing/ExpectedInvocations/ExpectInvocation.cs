namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;

    abstract class ExpectInvocation
    {
        public abstract void Validate(TestableMessageHandlerContext context);

        protected void Fail(string message)
        {
            throw new Exception(message);
        }
    }
}