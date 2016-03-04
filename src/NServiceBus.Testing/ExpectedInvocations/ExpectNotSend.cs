namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Linq;

    class ExpectNotSend<TMessage> : ExpectInvocation
    {
        public ExpectNotSend(Func<TMessage, SendOptions, bool> check)
        {
            this.check = check ?? ((message, options) => true);
        }

        public override void Validate(TestableMessageHandlerContext context)
        {
            var invokedMessages = context.SentMessages.Containing<TMessage>().ToList();

            if (invokedMessages.Any(invokedMessage => check(invokedMessage.Message, invokedMessage.Options)))
            {
                Fail($"Expected no message of type {typeof(TMessage).Name} to be sent but an outgoing message matching your constraints was found.");
            }
        }

        readonly Func<TMessage, SendOptions, bool> check;
    }
}