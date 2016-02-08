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
                Fail(sentMessages.Select(i => i.Message).ToList());
            }
        }

        readonly Func<TMessage, TimeSpan, bool> check;
    }
}