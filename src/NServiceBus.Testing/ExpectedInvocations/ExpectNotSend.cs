namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Collections.Generic;

    class ExpectNotSend<TMessage> : ExpectedNotMessageInvocation<TMessage, SendOptions>
    {
        public ExpectNotSend(Func<TMessage, SendOptions, bool> check)
            : base(check)
        {
        }

        protected override IEnumerable<OutgoingMessage<TMessage, SendOptions>> GetMessages(TestableMessageHandlerContext context)
        {
            return context.SentMessages.Containing<TMessage>();
        }
    }
}