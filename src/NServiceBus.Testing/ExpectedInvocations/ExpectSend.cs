namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;

    class ExpectSend<TMessage> : ExpectedMessageInvocation<TMessage>
    {
        public ExpectSend(Func<TMessage, bool> check)
            : base(check, c => c.SentMessages)
        {
        }
    }
}