namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Linq;

    class ExpectNotSendLocal<TMessage> : ExpectedMessageInvocation<TMessage>
    {
        private readonly Func<TMessage, bool> check;

        public ExpectNotSendLocal(Func<TMessage, bool> check)
            : base(check, c => c.SentMessages, true)
        {
            this.check = check;
        }

        internal override void Validate(TestableMessageHandlerContext context)
        {
            var invokedMessages = GetInvokedMessages(context);

            foreach (var invokedMessage in invokedMessages)
            {
                if (((SendOptions)invokedMessage.SendOptions).IsRoutingToThisEndpoint() && check((TMessage)invokedMessage.Message))
                {
                    Fail(invokedMessages.Select(i => i.Message).Cast<TMessage>());
                    return;
                }
            }
        }
    }
}