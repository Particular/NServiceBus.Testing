namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Linq;

    class ExpectSendToDestination<TMessage> : ExpectInvocation
    {
        private readonly Func<TMessage, string, bool> check;

        public ExpectSendToDestination(Func<TMessage, string, bool> check)
        {
            this.check = check ?? ((m, s) => true);
        }

        public override void Validate(TestableMessageHandlerContext context)
        {
            var sentMessages = context.SentMessages
                .Where(i => i.Message.GetType() == typeof(TMessage))
                .Where(i => string.IsNullOrWhiteSpace(i.Options.GetCorrelationId()) &&
                    !string.IsNullOrWhiteSpace(i.Options.GetDestination()))
                .ToList();

            if (!sentMessages.Any(i => check((TMessage) i.Message, i.Options.GetDestination())))
            {
                Fail(sentMessages.Select(i => i.Message).Cast<TMessage>().ToList());
            }
        }
    }
}