namespace NServiceBus.Testing
{
    using System;
    using System.Linq;
    using System.Runtime.ExceptionServices;

    class ExpectNotPublish<TMessage> : ExpectInvocation
    {
        public ExpectNotPublish(Func<TMessage, PublishOptions, bool> check = null)
        {
            this.check = check ?? ((message, options) => true);
        }

        public override void Validate(TestableMessageHandlerContext context, ExceptionDispatchInfo exceptionInfo)
        {
            var invokedMessages = context.PublishedMessages.Containing<TMessage>().ToList();

            if (invokedMessages.Any(invokedMessage => check(invokedMessage.Message, invokedMessage.Options)))
            {
                Fail($"Expected no message of type {typeof(TMessage).Name} to be published but an outgoing message matching your constraints was found.");
            }
        }

        readonly Func<TMessage, PublishOptions, bool> check;
    }
}