namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    class ExpectedMessageInvocation<TMessage> : ExpectedInvocation
    {
        internal ExpectedMessageInvocation(
            Func<TMessage, bool> check,
            Func<TestableMessageHandlerContext, IList<InvokedMessage>> messages,
            bool negate = false)
        {
            this.check = check;
            this.messages = messages;
            this.negate = negate;
        }

        internal override void Validate(TestableMessageHandlerContext context)
        {
            var invokedMessages = messages(context)
                .Where(i => i.Message.GetType().FullName.Replace("__impl", "").Replace("\\", "") == typeof(TMessage).FullName)
                .ToList();

            var found = false;

            if (check == null && invokedMessages.Any())
            {
                found = true;
            } 
            else if (invokedMessages.Any(invokedMessage => check((TMessage) invokedMessage.Message)))
            {
                found = true;
            }

            if ((found || negate) && (!negate || !found))
            {
                return;
            }

            Fail(invokedMessages.Select(i => i.Message).Cast<TMessage>().ToList());
        }

        readonly Func<TMessage, bool> check;

        readonly Func<TestableMessageHandlerContext, IList<InvokedMessage>> messages;

        readonly bool negate;
    }
}