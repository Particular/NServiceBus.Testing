namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Collections.Generic;

    class ExpectPublish<TMessage> : ExpectedMessageInvocation<TMessage>
    {
        public ExpectPublish(Func<TMessage, bool> check)
            : base(check)
        {
        }

        protected override List<TMessage> GetMessages(TestableMessageHandlerContext context)
        {
            return context.PublishedMessages.GetMessages<TMessage>();
        }
    }
}