namespace NServiceBus.Testing
{
    using System;
    using System.Linq;

    class ExpectSendToDestination<TMessage> : ExpectInvocation
    {
        readonly Func<TMessage, string, bool> check;

        public ExpectSendToDestination(Func<TMessage, string, bool> check = null)
        {
            this.check = check ?? ((m, s) => true);
        }

        public override void Validate(TestableMessageHandlerContext context)
        {
            var sentMessages = context.SentMessages.Containing<TMessage>()
                .Where(i => !string.IsNullOrWhiteSpace(i.Options.GetDestination()))
                .ToList();

            if (!sentMessages.Any(i => check(i.Message, i.Options.GetDestination())))
            {
                Fail($"Expected a message of type {typeof(TMessage).Name} to be sent to a specific destination but a message matching your constraints was not found.");
            }
        }
    }
}