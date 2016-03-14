namespace NServiceBus.Testing
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using NServiceBus.Extensibility;
    using NServiceBus.Persistence;
    using NServiceBus.Testing.ExpectedInvocations;

    class TestableMessageHandlerContext : IMessageHandlerContext
    {
        public TestableMessageHandlerContext(IMessageCreator messageCreator)
        {
            this.messageCreator = messageCreator;
        }

        public SentMessage<object>[] SentMessages => sentMessages.ToArray();

        public PublishedMessage<object>[] PublishedMessages => publishedMessages.ToArray();

        public TimeoutMessage<object>[] TimeoutMessages => timeoutMessages.ToArray();

        public RepliedMessage<object>[] RepliedMessages => repliedMessages.ToArray();

        public string[] ForwardedMessages => forwardedMessages.ToArray();

        public IDictionary<string, string> IncomingHeaders { get; } = new Dictionary<string, string>();

        public IList<ExpectInvocation> ExpectedInvocations { get; } = new List<ExpectInvocation>();

        public bool HandlerInvocationAborted { get; private set; }

        public bool HandleCurrentMessageLaterWasCalled { get; private set; }

        public string MessageId { get; set; }

        public string ReplyToAddress { get; }

        public IReadOnlyDictionary<string, string> MessageHeaders => new ReadOnlyDictionary<string, string>(IncomingHeaders);

        public ContextBag Extensions { get; } = new ContextBag();

        public SynchronizedStorageSession SynchronizedStorageSession { get; }

        public Task Send(object message, SendOptions options)
        {
            var headers = options.GetHeaders();

            if (headers.ContainsKey("NServiceBus.IsSagaTimeoutMessage"))
            {
                if (headers["NServiceBus.IsSagaTimeoutMessage"] == bool.TrueString)
                {
                    var within = GetWithin(options);

                    timeoutMessages.Enqueue(new TimeoutMessage<object>(message, options, within));
                }
            }

            sentMessages.Enqueue(new SentMessage<object>(message, options));

            return Task.FromResult(0);
        }

        public Task Send<T>(Action<T> messageConstructor, SendOptions options)
        {
            return Send(messageCreator.CreateInstance(messageConstructor), options);
        }

        public Task Publish(object message, PublishOptions options)
        {
            publishedMessages.Enqueue(new PublishedMessage<object>(message, options));
            return Task.FromResult(0);
        }

        public Task Publish<T>(Action<T> messageConstructor, PublishOptions publishOptions)
        {
            return Publish(messageCreator.CreateInstance(messageConstructor), publishOptions);
        }

        public Task Reply(object message, ReplyOptions options)
        {
            repliedMessages.Enqueue(new RepliedMessage<object>(message, options));
            return Task.FromResult(0);
        }

        public Task Reply<T>(Action<T> messageConstructor, ReplyOptions options)
        {
            return Reply(messageCreator.CreateInstance(messageConstructor), options);
        }

        public Task ForwardCurrentMessageTo(string destination)
        {
            forwardedMessages.Enqueue(destination);
            return Task.FromResult(0);
        }

        public Task HandleCurrentMessageLater()
        {
            HandleCurrentMessageLaterWasCalled = true;
            return Task.FromResult(0);
        }

        public void DoNotContinueDispatchingCurrentMessageToHandlers()
        {
            HandlerInvocationAborted = true;
        }

        public void Validate()
        {
            foreach (var e in ExpectedInvocations)
            {
                e.Validate(this);
            }

            Clear();
        }

        public void Clear()
        {
            ExpectedInvocations.Clear();

            repliedMessages = new ConcurrentQueue<RepliedMessage<object>>();
            sentMessages = new ConcurrentQueue<SentMessage<object>>();
            publishedMessages = new ConcurrentQueue<PublishedMessage<object>>();
            forwardedMessages = new ConcurrentQueue<string>();

            IncomingHeaders.Clear();
            MessageId = null;
            HandleCurrentMessageLaterWasCalled = false;
            HandlerInvocationAborted = false;
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

        ConcurrentQueue<SentMessage<object>> sentMessages = new ConcurrentQueue<SentMessage<object>>();
        ConcurrentQueue<PublishedMessage<object>> publishedMessages = new ConcurrentQueue<PublishedMessage<object>>();
        ConcurrentQueue<RepliedMessage<object>> repliedMessages = new ConcurrentQueue<RepliedMessage<object>>();
        ConcurrentQueue<TimeoutMessage<object>> timeoutMessages = new ConcurrentQueue<TimeoutMessage<object>>();
        ConcurrentQueue<string> forwardedMessages = new ConcurrentQueue<string>();

        IMessageCreator messageCreator;
    }

    class OutgoingMessage<TMessage, TOptions>
    {
        protected OutgoingMessage(TMessage message, TOptions options)
        {
            Message = message;
            Options = options;
        }

        public TMessage Message { get; }
        public TOptions Options { get; }
    }

    class SentMessage<TMessage> : OutgoingMessage<TMessage, SendOptions>
    {
        public SentMessage(TMessage message, SendOptions options) : base(message, options)
        {
        }
    }

    class PublishedMessage<TMessage> : OutgoingMessage<TMessage, PublishOptions>
    {
        public PublishedMessage(TMessage message, PublishOptions options) : base(message, options)
        {
        }
    }

    class RepliedMessage<TMessage> : OutgoingMessage<TMessage, ReplyOptions>
    {
        public RepliedMessage(TMessage message, ReplyOptions options) : base(message, options)
        {
        }
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