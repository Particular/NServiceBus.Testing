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
            // this is required to track timeouts across multiple 'When' invocations on a Saga test.
            // the context.TimeoutMessages only contains timeouts requested on the current context.
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
        internal TimeoutMessage<object>[] previousTimeouts = emptyTimeouts;
        static readonly TimeoutMessage<object>[] emptyTimeouts = new TimeoutMessage<object>[0];
    }
}