namespace NServiceBus.Testing
{
    using System;
    using System.Linq;
    using NServiceBus.Testing.ExpectedInvocations;

    class ExpectForwardCurrentMessageTo : ExpectInvocation
    {
        public ExpectForwardCurrentMessageTo(Func<string, bool> check)
        {
            this.check = check;
        }

        public override void Validate(TestableMessageHandlerContext context)
        {
            if (!context.ForwardedMessages.Any(m => check(m)))
            {
                Fail(Enumerable.Empty<object>());
            }
        }

        readonly Func<string, bool> check;
    }
}