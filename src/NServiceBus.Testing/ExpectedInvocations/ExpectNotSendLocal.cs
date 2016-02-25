namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;

    class ExpectNotSendLocal<TMessage> : ExpectedMessageInvocation<TMessage>
    {
        public ExpectNotSendLocal(Func<TMessage, bool> check)
            : base(check, c => c.SentMessages, true)
        {
        }

        internal override void Validate(TestableMessageHandlerContext context)
        {
            base.Validate(context);
        }
    }
}