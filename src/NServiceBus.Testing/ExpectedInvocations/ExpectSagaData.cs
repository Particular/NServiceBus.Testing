namespace NServiceBus.Testing.ExpectedInvocations
{
    using System;

    class ExpectSagaData<TSagaData> : ExpectInvocation where TSagaData : IContainSagaData
    {
        readonly Saga saga;
        Func<TSagaData, bool> check;

        public ExpectSagaData(Saga saga, Func<TSagaData, bool> check)
        {
            this.saga = saga;
            this.check = check;
        }

        public override void Validate(TestableMessageHandlerContext context)
        {
            if (!check((TSagaData)saga.Entity))
            {
                Fail("Expected saga data to match but it does not.");
            }
        }
    }
}