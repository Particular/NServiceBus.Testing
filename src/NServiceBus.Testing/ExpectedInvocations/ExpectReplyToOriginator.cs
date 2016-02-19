namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Linq;

    class ExpectReplyToOriginator<TMessage> : ExpectInvocation
    {
        public ExpectReplyToOriginator(Func<TMessage, bool> check = null, bool negate = false)
        {
            this.check = check;
            this.negate = negate;
        }

        internal override void Validate(TestableMessageHandlerContext context)
        {
            var invokedMessages = context.RepliedMessages
                .Where(i => i.Message.GetType() == typeof(TMessage))
                .Where(i => !string.IsNullOrWhiteSpace(((ReplyOptions)i.SendOptions).GetCorrelationId()) &&
                    !string.IsNullOrWhiteSpace(((ReplyOptions)i.SendOptions).GetDestination()))
                .ToList();

            var found = false;

            if (check == null && invokedMessages.Any())
            {
                found = true;
            }
            else if (invokedMessages.Any(i => check((TMessage)i.Message)))
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
        private readonly bool negate;
    }
}