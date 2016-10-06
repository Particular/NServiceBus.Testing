namespace NServiceBus.Testing
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Runtime.ExceptionServices;
    using System.Threading.Tasks;

    class TestingContext : TestableMessageHandlerContext
    {
        public TestingContext(IMessageCreator messageCreator = null) : base(messageCreator)
        {
        }

        public TestingContext(IMessageCreator messageCreator, TimeoutMessage<object>[] timeoutMessages) : base(messageCreator)
        {
            this.timeoutMessages = new ConcurrentQueue<TimeoutMessage<object>>(timeoutMessages);
        }

        public TimeoutMessage<object>[] TimeoutMessages => timeoutMessages.ToArray();

        public void AddExpectation(ExpectInvocation expectation)
        {
            expectedInvocations.Add(expectation);
        }

        public override Task Send(object message, SendOptions options)
        {
            var headers = options.GetHeaders();

            if (headers.ContainsKey(NServiceBus.Headers.IsSagaTimeoutMessage))
            {
                if (headers[NServiceBus.Headers.IsSagaTimeoutMessage] == bool.TrueString)
                {
                    timeoutMessages.Enqueue(GetTimeoutMessage(message, options));
                }
            }

            return base.Send(message, options);
        }

        public void Validate(ExceptionDispatchInfo info = null)
        {
            foreach (var e in expectedInvocations)
            {
                e.Validate(this, info);
            }
        }

        static TimeoutMessage<object> GetTimeoutMessage(object message, SendOptions options)
        {
            var within = options.GetDeliveryDelay();
            if (within.HasValue)
            {
                return new TimeoutMessage<object>(message, options, within.Value);
            }

            var dateTimeOffset = options.GetDeliveryDate();
            return new TimeoutMessage<object>(message, options, dateTimeOffset.Value);
        }

        IList<ExpectInvocation> expectedInvocations = new List<ExpectInvocation>();

        ConcurrentQueue<TimeoutMessage<object>> timeoutMessages = new ConcurrentQueue<TimeoutMessage<object>>();
    }
}