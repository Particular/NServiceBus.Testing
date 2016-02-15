namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;

    class ExpectedNotPublishInvocation<TMessage> : ExpectedMessageInvocation<TMessage>
    {
        public ExpectedNotPublishInvocation(Func<TMessage, bool> check)
            : base(check, c => c.PublishedMessages, true)
        {
        }
    }
}