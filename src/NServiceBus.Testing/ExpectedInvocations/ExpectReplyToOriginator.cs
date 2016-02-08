namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;
    using System.Linq;

    class ExpectReplyToOriginator<TMessage> : ExpectInvocation
    {
        public ExpectReplyToOriginator(Func<TMessage, bool> check = null)
        {
            this.check = check ?? (m => true);
        }

        public override void Validate(TestableMessageHandlerContext context)
        {
            var repliedMessages = context.RepliedMessages
                .Containing<TMessage>()
                .Where(i => !string.IsNullOrWhiteSpace(i.Options.GetCorrelationId()) &&
                    !string.IsNullOrWhiteSpace(i.Options.GetDestination()))
                .ToList();

            if (!repliedMessages.Any(i => check(i.Message)))
            {
                Fail(repliedMessages.Select(i => i.Message));
            }
        }

        readonly Func<TMessage, bool> check;
    }
}