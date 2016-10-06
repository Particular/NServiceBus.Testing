namespace NServiceBus.Testing
{
    using System.Runtime.ExceptionServices;

    class ExpectDoNotContinueDispatching : ExpectInvocation
    {
        public override void Validate(TestableMessageHandlerContext context, ExceptionDispatchInfo exceptionInfo)
        {
            if (!context.DoNotContinueDispatchingCurrentMessageToHandlersWasCalled)
            {
                Fail($"Expected `{nameof(context.DoNotContinueDispatchingCurrentMessageToHandlers)}` to be called on but it was not.");
            }
        }
    }
}