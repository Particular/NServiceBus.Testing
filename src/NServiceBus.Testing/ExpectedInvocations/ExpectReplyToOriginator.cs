#pragma warning disable 618
namespace NServiceBus.Testing
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
                Fail($"Expected a reply of type {typeof(TMessage).Name} but no message matching your constraints was sent.");
            }
        }

        readonly Func<TMessage, bool> check;
    }
}