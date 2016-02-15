namespace NServiceBus.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NServiceBus.Testing.ExpectedInvocations;

    class ExpectedForwardCurrentMessageTo : ExpectedInvocation
    {
        internal ExpectedForwardCurrentMessageTo(Func<string, bool> check, bool negate = false)
        {
            this.check = check;
            this.negate = negate;
        }

        internal override void Validate(TestableMessageHandlerContext context)
        {
            var found = context.ForwardedMessages.Any(m => check(m));

            if ((found || negate) && (!negate || !found))
            {
                return;
            }

            Fail(new List<InvokedMessage>());
        }

        readonly Func<string, bool> check;
        readonly bool negate;
    }

    class ExpectedNotForwardCurrentMessageTo : ExpectedForwardCurrentMessageTo
    {
        public ExpectedNotForwardCurrentMessageTo(Func<string, bool> check) : base(check, true)
        {
        }
    }
}