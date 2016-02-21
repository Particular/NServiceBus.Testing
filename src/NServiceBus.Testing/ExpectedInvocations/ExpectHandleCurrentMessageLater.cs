namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;

    class ExpectHandleCurrentMessageLater : ExpectedMessageInvocation<object>
    {
        public ExpectHandleCurrentMessageLater(bool negate = false) 
            : base(null, context => context.SentMessages, negate)
        {
        }

        internal override void Validate(TestableMessageHandlerContext context)
        {
            if (!context.HandleCurrentMessageLaterWasCalled)
            {
                Fail(GetInvokedMessages(context));
            }
        }
    }
}