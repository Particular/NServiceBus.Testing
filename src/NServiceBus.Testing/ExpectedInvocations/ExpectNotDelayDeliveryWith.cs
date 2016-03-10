namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Linq;

    class ExpectNotDelayDeliveryWith<TMessage> : ExpectInvocation
    {
        public ExpectNotDelayDeliveryWith(Func<TMessage, TimeSpan, bool> check = null)
        {
            this.check = check ?? ((m, t) => true);
        }

        public override void Validate(TestableMessageHandlerContext context)
        {
            var sentMessages = context.SentMessages
                .Containing<TMessage>()
                .Where(s => s.Options.GetDeliveryDelay().HasValue)
                .ToList();

            if (sentMessages.Any(s => check(s.Message, s.Options.GetDeliveryDelay().Value)))
            {
                Fail($"Expected no message of type {typeof(TMessage).Name} to be deferred but a message matching your constraints was deferred.");
            }
        }

        readonly Func<TMessage, TimeSpan, bool> check;
    }
}