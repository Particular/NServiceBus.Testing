namespace NServiceBus.Testing
{
    using System;
    using System.Linq;
    using System.Runtime.ExceptionServices;

    class ExpectDoNotDeliverBefore<TMessage> : ExpectInvocation
    {
        public ExpectDoNotDeliverBefore(Func<TMessage, DateTime, bool> check = null)
        {
            this.check = check ?? ((m, d) => true);
        }

        public override void Validate(TestableMessageHandlerContext context, ExceptionDispatchInfo exceptionInfo)
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