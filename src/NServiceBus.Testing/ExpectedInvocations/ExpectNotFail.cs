namespace NServiceBus.Testing
{
    using System;
    using System.Runtime.ExceptionServices;

    class ExpectNotFail<TException> : ExpectInvocation
        where TException : Exception
    {
        public ExpectNotFail(Func<TException, bool> check = null)
        {
            this.check = check ?? (e => true);
        }

        public override void Validate(TestableMessageHandlerContext context, ExceptionDispatchInfo exceptionInfo)
        {
            if (check(null))
            {
                Fail($"Expected a message of type {typeof(TException).Name} to be sent to a specific destination but a message matching your constraints was not found.");
            }
        }

        readonly Func<TException, bool> check;
    }
}