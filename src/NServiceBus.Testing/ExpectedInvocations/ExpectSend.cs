namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Linq;

    class ExpectSend<TMessage> : ExpectInvocation
    {
        public ExpectSend(Func<TMessage, SendOptions, bool> check)
        {
            this.check = check ?? ((message, options) => true);
        }

        public override void Validate(TestableMessageHandlerContext context)
        {
            var invokedMessages = context.SentMessages.Containing<TMessage>().ToList();

            if (invokedMessages.Any(invokedMessage => check(invokedMessage.Message, invokedMessage.Options)))
            {
                return;
            }

            Fail($"Expected a message of type {typeof(TMessage).Name} to be sent but no outgoing message matching your constraints was found.");
        }

        readonly Func<TMessage, SendOptions, bool> check;
    }
}