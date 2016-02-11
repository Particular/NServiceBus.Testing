namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Linq;
    using NServiceBus.DelayedDelivery;
    using NServiceBus.DeliveryConstraints;
    using NServiceBus.Extensibility;

    class ExpectedTimeoutInvocation<TMessage> : ExpectedInvocation
    {
        internal ExpectedTimeoutInvocation(Func<TMessage, TimeSpan, bool> check)
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
                DelayDeliveryWith constraint;

                if (!invokedMessage.SendOptions.GetExtensions().TryGetDeliveryConstraint(out constraint))
                {
                    continue;
                }

                if (check((TMessage) invokedMessage.Message, constraint.Delay))
                {
                    return;
                }
            }

            Fail(invokedMessages.Select(i => i.Message).Cast<TMessage>().ToList());
        }

        readonly Func<TMessage, TimeSpan, bool> check;
    }
}