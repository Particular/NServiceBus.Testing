namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Collections.Generic;

    class ExpectNotSend<TMessage> : ExpectedNotMessageInvocation<TMessage>
    {
        public ExpectNotSend(Func<TMessage, bool> check)
            : base(check)
        {
        }

        protected override List<TMessage> GetMessages(TestableMessageHandlerContext context)
        {
            return context.SentMessages.GetMessages<TMessage>();
        }
    }
}