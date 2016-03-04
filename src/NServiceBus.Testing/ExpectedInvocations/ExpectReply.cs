namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Collections.Generic;

    class ExpectReply<TMessage> : ExpectedMessageInvocation<TMessage, ReplyOptions>
    {
        public ExpectReply(Func<TMessage, ReplyOptions, bool> check)
            : base(check)
        {
        }

        protected override IEnumerable<OutgoingMessage<TMessage, ReplyOptions>> GetMessages(TestableMessageHandlerContext context)
        {
            return context.RepliedMessages.Containing<TMessage>();
        }
    }
}