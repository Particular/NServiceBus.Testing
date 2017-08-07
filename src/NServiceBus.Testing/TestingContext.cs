namespace NServiceBus.Testing
{
    using System.Collections.Generic;

    class TestingContext : TestableMessageHandlerContext
    {
        public TestingContext(IMessageCreator messageCreator = null) : base(messageCreator)
        {
        }

        public TestingContext(IMessageCreator messageCreator, TimeoutMessage<object>[] timeoutMessages) : base(messageCreator)
        {
            this.previousTimeouts = timeoutMessages;
        }

        public void AddExpectation(ExpectInvocation expectation)
        {
            expectedInvocations.Add(expectation);
        }

        public void Validate()
        {
            foreach (var e in expectedInvocations)
            {
                e.Validate(this);
            }
        }

        IList<ExpectInvocation> expectedInvocations = new List<ExpectInvocation>();
        internal TimeoutMessage<object>[] previousTimeouts;
    }
}