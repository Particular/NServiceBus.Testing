namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Linq;

    class ExpectDoNotDeliverBefore<TMessage> : ExpectInvocation
    {
        public ExpectDoNotDeliverBefore(Func<TMessage, DateTime, bool> check)
        {
            this.check = check;
        }

        public override void Validate(TestableMessageHandlerContext context)
        {
            var sentMessages = context.SentMessages
                .Containing<TMessage>()
                .Where(s => s.Options.GetDeliveryDate().HasValue)
                .ToList();

            if (!sentMessages.Any(s => check(s.Message, s.Options.GetDeliveryDate().Value.DateTime)))
            {
                Fail($"Expected no message of type {typeof(TMessage).Name} to be deferred but a message matching your constraints was deferred.");
            }
        }

        readonly Func<TMessage, DateTime, bool> check;
    }
}