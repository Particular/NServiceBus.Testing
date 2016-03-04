namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Linq;

    class ExpectDelayDeliveryWith<TMessage> : ExpectInvocation
    {
        public ExpectDelayDeliveryWith(Func<TMessage, TimeSpan, bool> check)
        {
            this.check = check ?? ((m, t) => true);
        }

        public override void Validate(TestableMessageHandlerContext context)
        {
            var sentMessages = context.SentMessages
                .Containing<TMessage>()
                .Where(s => s.Options.GetDeliveryDelay().HasValue)
                .ToList();

            if (!sentMessages.Any(s => check(s.Message, s.Options.GetDeliveryDelay().Value)))
            {
                Fail($"Expected a message of type {typeof(TMessage).Name} to be deferred but no message matching your constraints was deferred.");
            }
        }

        // Expected <TMessage> to be delayed, but no outgoing message matched with the expected configuration.
        // Delayed <TMessage> with 00:15:00
        // De
        readonly Func<TMessage, TimeSpan, bool> check;
    }
}