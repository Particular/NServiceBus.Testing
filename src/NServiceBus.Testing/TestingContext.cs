namespace NServiceBus.Testing
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NServiceBus.Testing.ExpectedInvocations;

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

            if (headers.ContainsKey(Headers.IsSagaTimeoutMessage))
            {
                if (headers[Headers.IsSagaTimeoutMessage] == bool.TrueString)
                {
                    var within = GetWithin(options);

                    timeoutMessages.Enqueue(new TimeoutMessage<object>(message, options, within));
                }
            }

            return base.Send(message, options);
        }

        public void Validate()
        {
            foreach (var e in expectedInvocations)
            {
                e.Validate(this);
            }
        }

        static TimeSpan GetWithin(SendOptions options)
        {
            var within = options.GetDeliveryDelay();

            if (!within.HasValue)
            {
                var dateTimeOffset = options.GetDeliveryDate();
                if (dateTimeOffset != null)
                {
                    within = dateTimeOffset.Value - DateTimeOffset.Now;
                }
            }

            if (!within.HasValue)
            {
                throw new Exception("No time has been set for the timeout message");
            }

            return within.Value;
        }

        IList<ExpectInvocation> expectedInvocations = new List<ExpectInvocation>();

        ConcurrentQueue<TimeoutMessage<object>> timeoutMessages = new ConcurrentQueue<TimeoutMessage<object>>();
    }

    class TimeoutMessage<TMessage> : OutgoingMessage<TMessage, SendOptions>
    {
        public TimeoutMessage(TMessage message, SendOptions options, TimeSpan within) : base(message, options)
        {
            Within = within;
        }

        public TimeSpan Within { get; private set; }
    }
}