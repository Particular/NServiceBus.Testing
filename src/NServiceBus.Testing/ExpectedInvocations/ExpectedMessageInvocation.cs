namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    abstract class ExpectedMessageInvocation<TMessage> : ExpectInvocation
    {
        protected ExpectedMessageInvocation(Func<TMessage, bool> check)
        {
            this.check = check ?? (message => true);
        }

        public override void Validate(TestableMessageHandlerContext context)
        {
            var invokedMessages = GetMessages(context);

            if (invokedMessages.Any(invokedMessage => check(invokedMessage)))
            {
                return;
            }

            Fail(invokedMessages);
        }

        protected abstract List<TMessage> GetMessages(TestableMessageHandlerContext context);

        readonly Func<TMessage, bool> check;
    }
}