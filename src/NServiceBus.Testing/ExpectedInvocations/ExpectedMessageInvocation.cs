namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    // Todo: Need to inherit from this class for the other messages types?
    class ExpectedMessageInvocation<TMessage> : ExpectedInvocation
    {
        internal ExpectedMessageInvocation(Func<TMessage, bool> check, Func<IList<InvokedMessage>> messages)
        {
            this.check = check;
            this.messages = messages;
        }

        internal override void Validate(TestableMessageHandlerContext context)
        {
           var invokedMessages = messages()
                .Where(i => i.Message.GetType().FullName.Replace("__impl", "").Replace("\\", "") == typeof(TMessage).FullName)
                .ToList();

            if (check == null && invokedMessages.Any()) return;

            if (invokedMessages.Any(invokedMessage => check((TMessage)invokedMessage.Message))) return;

            Fail(invokedMessages.Select(i => i.Message).Cast<TMessage>().ToList());
        }

        readonly Func<TMessage, bool> check;

        readonly Func<IList<InvokedMessage>> messages;
    }
}