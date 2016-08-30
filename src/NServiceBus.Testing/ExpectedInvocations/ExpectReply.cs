namespace NServiceBus.Testing
{
    using System;
    using System.Linq;
    using System.Runtime.ExceptionServices;

    class ExpectReply<TMessage> : ExpectInvocation
    {
        public ExpectReply(Func<TMessage, ReplyOptions, bool> check = null)
        {
            this.check = check ?? ((message, options) => true);
        }

        public override void Validate(TestableMessageHandlerContext context, ExceptionDispatchInfo exceptionInfo)
        {
            var invokedMessages = context.RepliedMessages.Containing<TMessage>().ToList();

            if (invokedMessages.Any(invokedMessage => check(invokedMessage.Message, invokedMessage.Options)))
            {
                return;
            }

            Fail($"Expected a reply of type {typeof(TMessage).Name} to be sent but no outgoing message matching your constraints was found.");
        }

        readonly Func<TMessage, ReplyOptions, bool> check;
    }
}