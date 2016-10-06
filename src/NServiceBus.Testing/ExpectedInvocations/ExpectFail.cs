namespace NServiceBus.Testing
{
    using System;
    using System.Runtime.ExceptionServices;

    class ExpectFail<TException> : ExpectInvocation
        where TException : Exception
    {
        public ExpectFail(Func<TException, bool> check = null)
        {
            this.check = check ?? (e => true);
        }

        public override void Validate(TestableMessageHandlerContext context, ExceptionDispatchInfo exceptionInfo)
        {
            if (exceptionInfo == null)
            {
                Fail($"Expected exception of type {typeof(TException).Name} but no exception was thrown.");
                return;
            }

            var exception = exceptionInfo.SourceException as TException;

            if (exception == null)
            {
                Fail($"Expected exception of type {typeof(TException).Name} but exception was of type { exceptionInfo.SourceException.GetType().Name}.");
                return;
            }

            if (!check(exception))
            {
                Fail($"Expected exception of type {typeof(TException).Name} to be thrown but the thrown exception did not match the constraint.");
            }
        }

        readonly Func<TException, bool> check;
    }
}