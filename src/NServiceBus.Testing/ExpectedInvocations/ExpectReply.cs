namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;

    class ExpectReply<TMessage> : ExpectedMessageInvocation<TMessage>
    {
        public ExpectReply(Func<TMessage, bool> check)
            : base(check, c => c.RepliedMessages)
        {
        }
    }
}