namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;

    class ExpectedNotReplyInvocation<TMessage> : ExpectedMessageInvocation<TMessage>
    {
        public ExpectedNotReplyInvocation(Func<TMessage, bool> check)
            : base(check, c => c.RepliedMessages, true)
        {
        }
    }
}