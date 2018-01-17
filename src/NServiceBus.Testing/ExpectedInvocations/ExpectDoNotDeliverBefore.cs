namespace NServiceBus.Testing
{
    using System;
    using System.Linq;

    class ExpectDoNotDeliverBefore<TMessage> : ExpectInvocation
    {
        public ExpectDoNotDeliverBefore(Func<TMessage, DateTimeOffset, bool> check = null)
        {
            this.check = check ?? ((m, d) => true);
        }

        public override void Validate(TestableMessageHandlerContext context)
        {
            var sentMessages = context.SentMessages
                .Containing<TMessage>()
                .Where(s => s.Options.GetDeliveryDate().HasValue)
                .ToList();

            if (!sentMessages.Any(s => check(s.Message, s.Options.GetDeliveryDate().Value)))
            {
                Fail($"Expected a message of type {typeof(TMessage).Name} to be deferred, but no message matching your constraints was deferred.");
            }
        }

        readonly Func<TMessage, DateTimeOffset, bool> check;
    }
}