namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Collections.Generic;

    class ExpectNotReply<TMessage> : ExpectedNotMessageInvocation<TMessage, ReplyOptions>
    {
        public ExpectNotReply(Func<TMessage, ReplyOptions, bool> check)
            : base(check)
        {
        }

        protected override IEnumerable<OutgoingMessage<TMessage, ReplyOptions>> GetMessages(TestableMessageHandlerContext context)
        {
            return context.RepliedMessages.Containing<TMessage>();
        }
    }
}