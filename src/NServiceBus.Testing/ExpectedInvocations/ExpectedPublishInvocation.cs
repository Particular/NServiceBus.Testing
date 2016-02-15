namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;

    class ExpectedPublishInvocation<TMessage> : ExpectedMessageInvocation<TMessage>
    {
        public ExpectedPublishInvocation(Func<TMessage, bool> check)
            : base(check, c => c.PublishedMessages)
        {
        }
    }
}