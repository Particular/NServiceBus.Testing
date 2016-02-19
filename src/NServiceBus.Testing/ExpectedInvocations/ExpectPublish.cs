namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;

    class ExpectPublish<TMessage> : ExpectedMessageInvocation<TMessage>
    {
        public ExpectPublish(Func<TMessage, bool> check)
            : base(check, c => c.PublishedMessages)
        {
        }
    }
}