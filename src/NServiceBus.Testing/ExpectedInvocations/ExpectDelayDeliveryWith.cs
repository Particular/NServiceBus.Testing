namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Linq;
    using NServiceBus.DelayedDelivery;
    using NServiceBus.DeliveryConstraints;
    using NServiceBus.Extensibility;

    class ExpectDelayDeliveryWith<TMessage> : ExpectInvocation
    {
        internal ExpectDelayDeliveryWith(Func<TMessage, TimeSpan, bool> check, bool negate = false)
        {
            this.check = check;
            this.negate = negate;
        }

        internal override void Validate(TestableMessageHandlerContext context)
        {
            var invokedMessages = context.SentMessages
                .Where(i => i.Message.GetType() == typeof(TMessage))
                .ToList();

            var found = false;

            if (check == null && invokedMessages.Any())
            {
                found = true;
            }
            else
            {
                foreach (var invokedMessage in invokedMessages)
                {
                    DelayDeliveryWith constraint;

                    ((SendOptions)invokedMessage.SendOptions).GetCorrelationId();

                    if (!invokedMessage.SendOptions.GetExtensions().TryGetDeliveryConstraint(out constraint))
                    {
                        continue;
                    }

                    if (check((TMessage)invokedMessage.Message, constraint.Delay))
                    {
                        found = true;
                    }
                }
            }

            if ((found || negate) && (!negate || !found))
            {
                return;
            }

            Fail(invokedMessages.Select(i => i.Message).Cast<TMessage>().ToList());
        }

        readonly Func<TMessage, TimeSpan, bool> check;
        readonly bool negate;
    }
}