namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;

    class ExpectedNotSendInvocation<TMessage> : ExpectedMessageInvocation<TMessage>
    {
        public ExpectedNotSendInvocation(Func<TMessage, bool> check)
            : base(check, c => c.SentMessages, true)
        {
        }
    }
}