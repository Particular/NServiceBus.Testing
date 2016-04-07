namespace NServiceBus.Testing
{
    class ExpectDoNotContinueDispatching : ExpectInvocation
    {
        public override void Validate(TestableMessageHandlerContext context)
        {
            if (!context.DoNotContinueDispatchingCurrentMessageToHandlersWasCalled)
            {
                Fail($"Expected `{nameof(context.DoNotContinueDispatchingCurrentMessageToHandlers)}` to be called on but it was not.");
            }
        }
    }
}