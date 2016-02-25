namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;

    class ExpectNotPublish<TMessage> : ExpectedNotMessageInvocation<TMessage>
    {
        public ExpectNotPublish(Func<TMessage, bool> check)
            : base(check, c => c.PublishedMessages)
        {
        }
    }
}