namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Linq;
    using NServiceBus.DelayedDelivery;
    using NServiceBus.DeliveryConstraints;
    using NServiceBus.Extensibility;

    class ExpectDoNotDeliverBefore<TMessage> : ExpectedMessageInvocation<TMessage>
    {
        internal ExpectDoNotDeliverBefore(Func<TMessage, DateTime, bool> check, bool negate = false) 
            : base(null, context => context.SentMessages, negate)
        {
            this.check = check;
            this.negate = negate;
        }

        internal override void Validate(TestableMessageHandlerContext context)
        {
            var invokedMessages = GetInvokedMessages(context);

            var found = false;
            if (check == null && invokedMessages.Any())
            {
                found = true;
            }
            else
            {
                foreach (var invokedMessage in invokedMessages)
                {
                    DoNotDeliverBefore constraint;

                    if (!invokedMessage.SendOptions.GetExtensions().TryGetDeliveryConstraint(out constraint))
                    {
                        continue;
                    }

                    if (check((TMessage)invokedMessage.Message, constraint.At))
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

        readonly Func<TMessage, DateTime, bool> check;
        readonly bool negate;
    }
}