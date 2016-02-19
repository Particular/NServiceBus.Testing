namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Linq;

    class ExpectSendToDestination<TMessage> : ExpectInvocation
    {
        private readonly Func<TMessage, string, bool> check;
        private readonly bool negate;

        internal ExpectSendToDestination(Func<TMessage, string, bool> check, bool negate = false)
        {
            this.check = check;
            this.negate = negate;
        }

        internal override void Validate(TestableMessageHandlerContext context)
        {
            var invokedMessages = context.SentMessages
                .Where(i => i.Message.GetType() == typeof(TMessage))
                .Where(i => string.IsNullOrWhiteSpace(((SendOptions)i.SendOptions).GetCorrelationId()) &&
                    !string.IsNullOrWhiteSpace(((SendOptions)i.SendOptions).GetDestination()))
                .ToList();

            var found = false;

            if (check == null && invokedMessages.Any())
            {
                found = true;
            }
            else if (invokedMessages.Any(i => check((TMessage)i.Message, ((SendOptions)i.SendOptions).GetDestination())))
            {
                found = true;
            }

            if ((found || negate) && (!negate || !found))
            {
                return;
            }

            Fail(invokedMessages.Select(i => i.Message).Cast<TMessage>().ToList());
        }
    }
}