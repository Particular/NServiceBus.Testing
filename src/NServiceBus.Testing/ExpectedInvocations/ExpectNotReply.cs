namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Collections.Generic;

    class ExpectNotReply<TMessage> : ExpectedNotMessageInvocation<TMessage>
    {
        public ExpectNotReply(Func<TMessage, bool> check)
            : base(check)
        {
        }

        protected override List<TMessage> GetMessages(TestableMessageHandlerContext context)
        {
            return context.RepliedMessages.GetMessages<TMessage>();
        }
    }
}