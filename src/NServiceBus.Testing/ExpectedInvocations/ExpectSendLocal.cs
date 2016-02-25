namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;

    class ExpectSendLocal<TMessage> : ExpectedMessageInvocation<TMessage>
    {
        public ExpectSendLocal(Func<TMessage, bool> check)
            : base(check, c => c.SentMessages)
        {
        }

        internal override void Validate(TestableMessageHandlerContext context)
        {
            var invokedMessages = GetInvokedMessages(context);

            foreach (var invokedMessage in invokedMessages)
            {
                ((SendOptions)invokedMessage.SendOptions).GetCorrelationId();
            }
        }
    }
}