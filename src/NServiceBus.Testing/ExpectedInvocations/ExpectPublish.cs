namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Collections.Generic;

    class ExpectPublish<TMessage> : ExpectedMessageInvocation<TMessage, PublishOptions>
    {
        public ExpectPublish(Func<TMessage, PublishOptions, bool> check)
            : base(check)
        {
        }

        protected override IEnumerable<OutgoingMessage<TMessage, PublishOptions>> GetMessages(TestableMessageHandlerContext context)
        {
            return context.PublishedMessages.Containing<TMessage>();
        }
    }
}