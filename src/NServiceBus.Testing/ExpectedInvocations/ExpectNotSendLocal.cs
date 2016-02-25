namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Linq;

    class ExpectNotSendLocal<TMessage> : ExpectInvocation
    {
        private readonly Func<TMessage, bool> check;

        public ExpectNotSendLocal(Func<TMessage, bool> check)
        {
            this.check = check;
        }

        internal override void Validate(TestableMessageHandlerContext context)
        {
            var invokedMessages = context.SentMessages.Containing<TMessage>();

            foreach (var invokedMessage in invokedMessages)
            {
                if (invokedMessage.Options.IsRoutingToThisEndpoint() && check(invokedMessage.Message))
                {
                    Fail(invokedMessages.Select(i => i.Message));
                    return;
                }
            }
        }
    }
}