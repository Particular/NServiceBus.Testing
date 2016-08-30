namespace NServiceBus.Testing
{
    using System.Runtime.ExceptionServices;

    class ExpectHandleCurrentMessageLater : ExpectInvocation
    {
        public override void Validate(TestableMessageHandlerContext context, ExceptionDispatchInfo exceptionInfo)
        {
            if (!context.HandleCurrentMessageLaterWasCalled)
            {
                Fail($"Expected {nameof(context.HandleCurrentMessageLaterWasCalled)} to be called but it was not.");
            }
        }
    }
}