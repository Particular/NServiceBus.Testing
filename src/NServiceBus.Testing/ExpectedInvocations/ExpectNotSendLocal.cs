namespace NServiceBus.Testing
{
    using System;
    using System.Linq;
    using System.Runtime.ExceptionServices;

    class ExpectNotSendLocal<TMessage> : ExpectInvocation
    {
        public ExpectNotSendLocal(Func<TMessage, bool> check = null)
        {
            this.check = check ?? (x => true);
        }

        public override void Validate(TestableMessageHandlerContext context, ExceptionDispatchInfo exceptionInfo)
        {
            var sentMessages = context.SentMessages.Containing<TMessage>();

            if (sentMessages.Any(s => s.Options.IsRoutingToThisEndpoint() && check(s.Message)))
            {
                Fail($"Expected no message of type {typeof(TMessage).Name} to be routed to the current endpoint but a message matching your constraints was found.");
            }
        }

        readonly Func<TMessage, bool> check;
    }
}