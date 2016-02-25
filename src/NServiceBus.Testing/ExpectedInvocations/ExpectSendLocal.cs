namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Linq;

    class ExpectSendLocal<TMessage> : ExpectInvocation
    {
        private readonly Func<TMessage, bool> check;

        public ExpectSendLocal(Func<TMessage, bool> check)
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
                    return;
                }
            }

            Fail(invokedMessages.Select(i => i.Message));
        }
    }
}