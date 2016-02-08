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
                Fail(context.ForwardedMessages);
            }
        }

        readonly Func<string, bool> check;
    }
}