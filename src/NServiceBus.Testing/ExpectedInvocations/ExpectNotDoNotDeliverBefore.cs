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
                Fail(sentMessages.Select(i => i.Message).ToList());
            }
        }

        readonly Func<TMessage, DateTime, bool> check;
    }
}