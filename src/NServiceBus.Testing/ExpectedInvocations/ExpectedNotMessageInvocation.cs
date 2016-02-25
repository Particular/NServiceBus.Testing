namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    class ExpectedNotMessageInvocation<TMessage> : ExpectInvocation
    {
        internal ExpectedNotMessageInvocation(
            Func<TMessage, bool> check,
            Func<TestableMessageHandlerContext, IList<InvokedMessage>> messages)
        {
            this.check = check ?? (message => true);
            this.messages = messages;

        }

        internal override void Validate(TestableMessageHandlerContext context)
        {
            var invokedMessages = messages(context).Containing<TMessage>();

            if (invokedMessages.Any(invokedMessage => check((TMessage)invokedMessage.Message)))
            {
                Fail(invokedMessages.Select(i => i.Message).Cast<TMessage>().ToList());

            }

            return;
        }

        readonly Func<TMessage, bool> check;

        readonly Func<TestableMessageHandlerContext, IList<InvokedMessage>> messages;
    }
}