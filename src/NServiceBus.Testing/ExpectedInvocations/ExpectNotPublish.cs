namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Linq;

    class ExpectNotPublish<TMessage> : ExpectInvocation
    {
        public ExpectNotPublish(Func<TMessage, PublishOptions, bool> check)
        {
            this.check = check ?? ((message, options) => true);
        }

        public override void Validate(TestableMessageHandlerContext context)
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