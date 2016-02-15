namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Linq;
    using NServiceBus.DelayedDelivery;
    using NServiceBus.DeliveryConstraints;
    using NServiceBus.Extensibility;

    class ExpectedDoNotDeliverBeforeInvocation<TMessage> : ExpectedInvocation
    {
        internal ExpectedDoNotDeliverBeforeInvocation(Func<TMessage, DateTime, bool> check)
        {
            this.check = check;
        }

        internal override void Validate(TestableMessageHandlerContext context)
        {
            var invokedMessages = context.SentMessages
                .Where(i => i.Message.GetType() == typeof(TMessage))
                .ToList();

            if (check == null && invokedMessages.Any())
            {
                return;
            }

            foreach (var invokedMessage in invokedMessages)
            {
                DoNotDeliverBefore constraint;

                if (!invokedMessage.SendOptions.GetExtensions().TryGetDeliveryConstraint(out constraint))
                {
                    continue;
                }

                if (check((TMessage) invokedMessage.Message, constraint.At))
                {
                    return;
                }
            }

            Fail(invokedMessages.Select(i => i.Message).Cast<TMessage>().ToList());
        }

        readonly Func<TMessage, DateTime, bool> check;
    }
}