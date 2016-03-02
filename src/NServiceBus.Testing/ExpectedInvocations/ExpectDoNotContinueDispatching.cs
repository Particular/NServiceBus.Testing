namespace NServiceBus.Testing.ExpectedInvocations
{
    internal class ExpectDoNotContinueDispatching : ExpectInvocation
    {
        public override void Validate(TestableMessageHandlerContext context)
        {
            if (!context.HandlerInvocationAborted)
            {
                Fail("DoNotContinueDispatching was not called.");
            }
        }
    }
}