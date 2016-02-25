﻿namespace NServiceBus.Testing
{
    using System;
    using System.Linq;
    using NServiceBus.Testing.ExpectedInvocations;

    class ExpectForwardCurrentMessageTo : ExpectInvocation
    {
        internal ExpectForwardCurrentMessageTo(Func<string, bool> check, bool negate = false)
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

            Fail(Enumerable.Empty<object>());
        }

        readonly Func<string, bool> check;
        readonly bool negate;
    }
}