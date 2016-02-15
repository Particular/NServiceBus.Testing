namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;

    class ExpectedSendInvocation<TMessage> : ExpectedMessageInvocation<TMessage>
    {
        public ExpectedSendInvocation(Func<TMessage, bool> check)
            : base(check, c => c.SentMessages)
        {
        }
    }
}