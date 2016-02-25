namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    class ExpectedMessageInvocation<TMessage> : ExpectInvocation
    {
        internal ExpectedMessageInvocation(
            Func<TMessage, bool> check,
            Func<TestableMessageHandlerContext, IList<InvokedMessage>> messages)
        {
            this.check = check ?? (message => true);
            this.messages = messages;
            
        }

        internal override void Validate(TestableMessageHandlerContext context)
        {
            var invokedMessages = messages(context).Containing<TMessage>();

            if (invokedMessages.Any(invokedMessage => check((TMessage) invokedMessage.Message)))
            {
                return;
            }

            Fail(invokedMessages.Select(i => i.Message).Cast<TMessage>().ToList());
        }

        readonly Func<TMessage, bool> check;

        readonly Func<TestableMessageHandlerContext, IList<InvokedMessage>> messages;
    }
}