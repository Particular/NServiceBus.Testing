namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Linq;

    internal class ExpectDoNotContinueDispatching : ExpectInvocation
    {
        internal override void Validate(TestableMessageHandlerContext context)
        {
            if (!context.HandlerInvocationAborted)
            {
                Fail(Enumerable.Empty<object>().ToList());
            }
        }
    }
}