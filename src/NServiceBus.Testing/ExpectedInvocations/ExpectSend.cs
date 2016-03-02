namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Collections.Generic;

    class ExpectSend<TMessage> : ExpectedMessageInvocation<TMessage>
    {
        public ExpectSend(Func<TMessage, bool> check)
            : base(check)
        {
        }

        protected override List<TMessage> GetMessages(TestableMessageHandlerContext context)
        {
            return context.SentMessages.GetMessages<TMessage>();
        }
    }
}