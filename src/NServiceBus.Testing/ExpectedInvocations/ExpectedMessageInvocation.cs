namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    abstract class ExpectedMessageInvocation<TMessage, TOptions> : ExpectInvocation
    {
        protected ExpectedMessageInvocation(Func<TMessage, TOptions, bool> check)
        {
            this.check = check ?? ((message, options) => true);
        }

        public override void Validate(TestableMessageHandlerContext context)
        {
            var invokedMessages = GetMessages(context).ToList();

            if (invokedMessages.Any(invokedMessage => check(invokedMessage.Message, invokedMessage.Options)))
            {
                return;
            }

            Fail(invokedMessages.Select(x => x.Message));
        }

        protected abstract IEnumerable<OutgoingMessage<TMessage, TOptions>> GetMessages(TestableMessageHandlerContext context);

        readonly Func<TMessage, TOptions, bool> check;
    }
}