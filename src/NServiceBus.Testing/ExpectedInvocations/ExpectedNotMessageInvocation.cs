namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    abstract class ExpectedNotMessageInvocation<TMessage, TOptions> : ExpectInvocation
    {
        protected ExpectedNotMessageInvocation(Func<TMessage, TOptions, bool> check)
        {
            this.check = check ?? ((message, options) => true);
        }

        public override void Validate(TestableMessageHandlerContext context)
        {
            var invokedMessages = GetMessages(context).ToList();

            if (invokedMessages.Any(invokedMessage => check(invokedMessage.Message, invokedMessage.Options)))
            {
                Fail(invokedMessages.Select(x => x.Message));
            }
        }

        protected abstract IEnumerable<OutgoingMessage<TMessage, TOptions>> GetMessages(TestableMessageHandlerContext context);

        readonly Func<TMessage, TOptions, bool> check;
    }
}