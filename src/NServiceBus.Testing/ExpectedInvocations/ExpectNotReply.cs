namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;

    class ExpectNotReply<TMessage> : ExpectedNotMessageInvocation<TMessage>
    {
        public ExpectNotReply(Func<TMessage, bool> check)
            : base(check, c => c.RepliedMessages)
        {
        }
    }
}