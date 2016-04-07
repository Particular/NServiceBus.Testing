namespace NServiceBus.Testing
{
    using System;
    using System.Linq;

    class ExpectNotReply<TMessage> : ExpectInvocation
    {
        public ExpectNotReply(Func<TMessage, ReplyOptions, bool> check = null)
        {
            this.check = check ?? ((message, options) => true);
        }

        public override void Validate(TestableMessageHandlerContext context)
        {
            var invokedMessages = context.RepliedMessages.Containing<TMessage>().ToList();

            if (invokedMessages.Any(invokedMessage => check(invokedMessage.Message, invokedMessage.Options)))
            {
                Fail($"Expected no reply of type {typeof(TMessage).Name} to be sent but an outgoing message matching your constraints was found.");
            }
        }

        readonly Func<TMessage, ReplyOptions, bool> check;
    }
}