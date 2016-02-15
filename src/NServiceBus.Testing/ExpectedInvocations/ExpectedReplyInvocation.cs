namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;

    class ExpectedReplyInvocation<TMessage> : ExpectedMessageInvocation<TMessage>
    {
        public ExpectedReplyInvocation(Func<TMessage, bool> check)
            : base(check, c => c.RepliedMessages)
        {
        }
    }
}