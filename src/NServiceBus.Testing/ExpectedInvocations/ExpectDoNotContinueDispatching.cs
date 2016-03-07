namespace NServiceBus.Testing.ExpectedInvocations
{
    class ExpectDoNotContinueDispatching : ExpectInvocation
    {
        public override void Validate(TestableMessageHandlerContext context)
        {
            if (!context.HandlerInvocationAborted)
            {
                Fail($"Expected `{nameof(context.DoNotContinueDispatchingCurrentMessageToHandlers)}` to be called on but it was not.");
            }
        }
    }
}