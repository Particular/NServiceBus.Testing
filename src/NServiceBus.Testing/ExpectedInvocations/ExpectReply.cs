namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Collections.Generic;

    class ExpectReply<TMessage> : ExpectedMessageInvocation<TMessage>
    {
        public ExpectReply(Func<TMessage, bool> check)
            : base(check)
        {
        }

        protected override List<TMessage> GetMessages(TestableMessageHandlerContext context)
        {
            return context.RepliedMessages.GetMessages<TMessage>();
        }
    }
}