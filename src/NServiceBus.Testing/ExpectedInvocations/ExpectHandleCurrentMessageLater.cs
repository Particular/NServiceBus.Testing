namespace NServiceBus.Testing.ExpectedInvocations
{
    class ExpectHandleCurrentMessageLater : ExpectInvocation
    {
        internal override void Validate(TestableMessageHandlerContext context)
        {
            if (!context.HandleCurrentMessageLaterWasCalled)
            {
                Fail(context.SentMessages);
            }
        }
    }
}