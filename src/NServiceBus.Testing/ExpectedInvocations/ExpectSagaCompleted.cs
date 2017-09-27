namespace NServiceBus.Testing
{
    class ExpectSagaCompleted<TSaga> : ExpectInvocation where TSaga : Saga
    {
        readonly TSaga saga;
        readonly bool expectCompleted;

        public ExpectSagaCompleted(TSaga saga, bool expectCompleted)
        {
            this.saga = saga;
            this.expectCompleted = expectCompleted;
        }

        public override void Validate(TestableMessageHandlerContext context)
        {
            if (saga.Completed == expectCompleted)
            {
                return;
            }

            if (saga.Completed)
            {
                Fail("Expected saga to not be completed but the saga was completed.");
            }

            Fail("Expected saga to be completed but the saga was not completed.");
        }
    }
}