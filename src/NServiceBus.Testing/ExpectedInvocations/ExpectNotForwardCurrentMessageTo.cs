namespace NServiceBus.Testing
{
    using System;
    using System.Linq;
    using NServiceBus.Testing.ExpectedInvocations;

    class ExpectNotForwardCurrentMessageTo : ExpectInvocation
    {
        public ExpectNotForwardCurrentMessageTo(Func<string, bool> check)
        {
            this.check = check;
        }

        public override void Validate(TestableMessageHandlerContext context)
        {
            if (context.ForwardedMessages.Any(m => check(m)))
            {
                Fail("Expected the incoming message not to be forwarded but a forwarded message matching your constraints was found.");
            }
        }

        readonly Func<string, bool> check;
    }
}