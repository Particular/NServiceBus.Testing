namespace NServiceBus.Testing
{
    using System;

    class ExpectNotForwardCurrentMessageTo : ExpectForwardCurrentMessageTo
    {
        public ExpectNotForwardCurrentMessageTo(Func<string, bool> check) : base(check, true)
        {
        }
    }
}