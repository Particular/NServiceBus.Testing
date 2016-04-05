namespace NServiceBus.Testing
{
    using System;
    using System.Linq;
    using ExpectedInvocations;

    class ExpectForwardCurrentMessageTo : ExpectInvocation
    {
        public ExpectForwardCurrentMessageTo(Func<string, bool> check = null)
        {
            this.check = check ?? (s => true);
        }

        public override void Validate(TestableMessageHandlerContext context)
        {
            if (!context.ForwardedMessages.Any(m => check(m)))
            {
                Fail("Expected the incoming message to be forwarded but no forwarded message matching your constraints was found.");
            }
        }

        readonly Func<string, bool> check;
    }
}