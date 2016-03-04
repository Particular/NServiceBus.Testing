namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Linq;

    class ExpectNotSendLocal<TMessage> : ExpectInvocation
    {
        readonly Func<TMessage, bool> check;

        public ExpectNotSendLocal(Func<TMessage, bool> check)
        {
            this.check = check;
        }

        public override void Validate(TestableMessageHandlerContext context)
        {
            var sentMessages = context.SentMessages.Containing<TMessage>();

            if (sentMessages.Any(s => s.Options.IsRoutingToThisEndpoint() && check(s.Message)))
            {
                Fail($"Expected no message of type {typeof(TMessage).Name} to be routed to the current endpoint but a message matching your constraints was found.");
            }
        }
    }
}