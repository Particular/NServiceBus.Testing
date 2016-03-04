namespace NServiceBus.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using NServiceBus.Extensibility;
    using NServiceBus.Persistence;
    using NServiceBus.Testing.ExpectedInvocations;

    internal class TestableMessageHandlerContext : IMessageHandlerContext
    {
        public TestableMessageHandlerContext(IMessageCreator messageCreator)
        {
            this.messageCreator = messageCreator;
        }
        public IList<SentMessage<object>> SentMessages { get; } = new List<SentMessage<object>>();

        public IList<PublishedMessage<object>> PublishedMessages { get; } = new List<PublishedMessage<object>>();

        public IList<RepliedMessage<object>> RepliedMessages { get; set; } = new List<RepliedMessage<object>>();

        public IList<string> ForwardedMessages { get; set; } = new List<string>();

        public IDictionary<string, string> IncomingHeaders { get; } = new Dictionary<string, string>();

        public IList<ExpectInvocation> ExpectedInvocations { get; } = new List<ExpectInvocation>();

        public string MessageId { get; set; }

        public string ReplyToAddress { get; }

        public IReadOnlyDictionary<string, string> MessageHeaders => new ReadOnlyDictionary<string, string>(IncomingHeaders);

        public ContextBag Extensions { get; } = new ContextBag();

        public SynchronizedStorageSession SynchronizedStorageSession { get; }

        public bool HandlerInvocationAborted { get; private set; }

        public bool HandleCurrentMessageLaterWasCalled { get; private set; }

        public Task Send(object message, SendOptions options)
        {
            SentMessages.Add(new SentMessage<object>(message, options));
            return Task.FromResult(0);
        }

        public Task Send<T>(Action<T> messageConstructor, SendOptions options)
        {
            return Send(messageCreator.CreateInstance(messageConstructor), options);
        }

        public Task Publish(object message, PublishOptions options)
        {
            PublishedMessages.Add(new PublishedMessage<object>(message, options));
            return Task.FromResult(0);
        }

        public Task Publish<T>(Action<T> messageConstructor, PublishOptions publishOptions)
        {
            return Publish(messageCreator.CreateInstance(messageConstructor), publishOptions);
        }

        public Task Reply(object message, ReplyOptions options)
        {
            RepliedMessages.Add(new RepliedMessage<object>(message, options));
            return Task.FromResult(0);
        }

        public Task Reply<T>(Action<T> messageConstructor, ReplyOptions options)
        {
            return Reply(messageCreator.CreateInstance(messageConstructor), options);
        }

        public Task ForwardCurrentMessageTo(string destination)
        {
            ForwardedMessages.Add(destination);
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
            try
            {
                foreach (var e in ExpectedInvocations)
                {
                    e.Validate(this);
                }
            }
            finally
            {
                Clear();
            }
        }

        public void Clear()
        {
            ExpectedInvocations.Clear();
            RepliedMessages.Clear();
            SentMessages.Clear();
            PublishedMessages.Clear();
            ForwardedMessages.Clear();
            IncomingHeaders.Clear();
            MessageId = null;
            HandleCurrentMessageLaterWasCalled = false;
            HandlerInvocationAborted = false;
        }

        IMessageCreator messageCreator;
    }

    internal class OutgoingMessage<TMessage, TOptions>
    {
        public OutgoingMessage(TMessage message, TOptions options)
        {
            Message = message;
            Options = options;
        }

        public TMessage Message { get; }
        public TOptions Options { get; }
    }

    internal class SentMessage<TMessage> : OutgoingMessage<TMessage, SendOptions>
    {
        internal SentMessage(TMessage message, SendOptions options) : base(message, options)
        {
        }
    }

    internal class PublishedMessage<TMessage> : OutgoingMessage<TMessage, PublishOptions>
    {
        internal PublishedMessage(TMessage message, PublishOptions options) : base(message, options)
        {
        }
    }

    internal class RepliedMessage<TMessage> : OutgoingMessage<TMessage, ReplyOptions>
    {
        internal RepliedMessage(TMessage message, ReplyOptions options) : base(message, options)
        {
        }
    }
}