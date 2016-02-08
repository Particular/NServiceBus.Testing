namespace NServiceBus.Testing.ExpectedInvocations
{
    class ExpectHandleCurrentMessageLater : ExpectInvocation
    {
        public override void Validate(TestableMessageHandlerContext context)
        {
            if (!context.HandleCurrentMessageLaterWasCalled)
            {
                Fail("HandleCurrentMessageLater was not called.");
            }
        }
    }
}