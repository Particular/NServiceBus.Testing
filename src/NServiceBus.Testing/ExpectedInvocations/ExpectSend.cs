namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Collections.Generic;

    class ExpectSend<TMessage> : ExpectedMessageInvocation<TMessage, SendOptions>
    {
        public ExpectSend(Func<TMessage, SendOptions, bool> check) : base(check)
        {
        }

        protected override IEnumerable<OutgoingMessage<TMessage, SendOptions>> GetMessages(TestableMessageHandlerContext context)
        {
            return context.SentMessages.Containing<TMessage>();
        }
    }
}