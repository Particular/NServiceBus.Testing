namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Linq;

    class ExpectPublish<TMessage> : ExpectInvocation
    {
        public ExpectPublish(Func<TMessage, PublishOptions, bool> check)
        {
            this.check = check ?? ((message, options) => true);
        }

        public override void Validate(TestableMessageHandlerContext context)
        {
            var invokedMessages = context.PublishedMessages.Containing<TMessage>().ToList();

            if (invokedMessages.Any(invokedMessage => check(invokedMessage.Message, invokedMessage.Options)))
            {
                return;
            }

            Fail($"Expected a message of type {typeof(TMessage).Name} to be published but no outgoing message matching your constraints was found.");
        }

        readonly Func<TMessage, PublishOptions, bool> check;
    }
}