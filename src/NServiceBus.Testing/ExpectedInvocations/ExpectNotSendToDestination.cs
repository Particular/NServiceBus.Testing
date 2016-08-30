namespace NServiceBus.Testing
{
    using System;
    using System.Linq;
    using System.Runtime.ExceptionServices;

    class ExpectNotSendToDestination<TMessage> : ExpectInvocation
    {
        readonly Func<TMessage, string, bool> check;

        public ExpectNotSendToDestination(Func<TMessage, string, bool> check = null)
        {
            this.check = check ?? ((m, s) => true);
        }

        public override void Validate(TestableMessageHandlerContext context, ExceptionDispatchInfo exceptionInfo)
        {
            var sentMessages = context.SentMessages.Containing<TMessage>()
                .Where(i => string.IsNullOrWhiteSpace(i.Options.GetCorrelationId()) &&
                    !string.IsNullOrWhiteSpace(i.Options.GetDestination()))
                .ToList();

            if (sentMessages.Any(i => check(i.Message, i.Options.GetDestination())))
            {
                Fail($"Expected no message of type {typeof(TMessage).Name} to be sent to a specific destination but a message matching your constraints was found.");
            }
        }
    }
}