namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;

    class ExpectNotSend<TMessage> : ExpectedMessageInvocation<TMessage>
    {
        public ExpectNotSend(Func<TMessage, bool> check)
            : base(check, c => c.SentMessages, true)
        {
        }
    }
}