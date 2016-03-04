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

        public override void Validate(TestableMessageHandlerContext context)
        {
            var sentMessages = context.SentMessages.Containing<TMessage>();

            if (!sentMessages.Any(s => s.Options.IsRoutingToThisEndpoint() && check(s.Message)))
            {
                Fail($"Expected a message of type {typeof(TMessage).Name} to be routed to the current endpoint but no message matching your constraints was found.");
            }
        }
    }
}