namespace NServiceBus.Testing
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Threading.Tasks;
    using NServiceBus.Extensibility;
    using NServiceBus.MessageInterfaces.MessageMapper.Reflection;
    using NServiceBus.Persistence;

    /// <summary>
    /// A testable implementation of <see cref="IMessageHandlerContext" />.
    /// </summary>
    public class TestableMessageHandlerContext : IMessageHandlerContext
    {
        /// <summary>
        /// Creates a new instance of a <see cref="TestableMessageHandlerContext" />.
        /// </summary>
        public TestableMessageHandlerContext(IMessageCreator messageCreator = null)
        {
            this.messageCreator = messageCreator ?? new MessageMapper();
        }

        /// <summary>
        /// A list of all messages sent by <see cref="IPipelineContext.Send" />.
        /// </summary>
        public virtual SentMessage<object>[] SentMessages => sentMessages.ToArray();

        /// <summary>
        /// A list of all messages published by <see cref="IPipelineContext.Publish" />,
        /// </summary>
        public virtual PublishedMessage<object>[] PublishedMessages => publishedMessages.ToArray();

        /// <summary>
        /// A list of all messages sent by <see cref="IMessageProcessingContext.Reply" />.
        /// </summary>
        public virtual RepliedMessage<object>[] RepliedMessages => repliedMessages.ToArray();

        /// <summary>
        /// A list of all forwarding destinations set by <see cref="IMessageProcessingContext.ForwardCurrentMessageTo" />.
        /// </summary>
        public virtual string[] ForwardedMessages => forwardedMessages.ToArray();

        /// <summary>
        /// The headers of the incoming message.
        /// </summary>
        public virtual IDictionary<string, string> MessageHeaders { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Indicates if <see cref="IMessageHandlerContext.DoNotContinueDispatchingCurrentMessageToHandlers" /> has been called.
        /// </summary>
        public virtual bool DoNotContinueDispatchingCurrentMessageToHandlersWasCalled { get; private set; }

        /// <summary>
        /// Indicates if <see cref="IMessageHandlerContext.HandleCurrentMessageLater" /> has been called.
        /// </summary>
        public virtual bool HandleCurrentMessageLaterWasCalled { get; private set; }

        /// <summary>
        /// The unique message ID of the incoming message.
        /// </summary>
        public virtual string MessageId { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The reply address for the incoming message.
        /// </summary>
        public virtual string ReplyToAddress { get; set; }

        IReadOnlyDictionary<string, string> IMessageProcessingContext.MessageHeaders => new ReadOnlyDictionary<string, string>(MessageHeaders);

        /// <summary>
        /// The extensions bag for the pipeline of the incoming message.
        /// </summary>
        public virtual ContextBag Extensions { get; set; } = new ContextBag();

        /// <summary>
        /// The <see cref="SynchronizedStorageSession" /> for the pipeline of the incoming message.
        /// </summary>
        public virtual SynchronizedStorageSession SynchronizedStorageSession { get; } = null;

        /// <summary>
        /// Sends the provided message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="options">The options for the send.</param>
        public virtual Task Send(object message, SendOptions options)
        {
            sentMessages.Enqueue(new SentMessage<object>(message, options));

            return Task.FromResult(0);
        }

        /// <summary>
        /// Instantiates a message of type T and sends it.
        /// </summary>
        /// <typeparam name="T">The type of message, usually an interface.</typeparam>
        /// <param name="messageConstructor">An action which initializes properties of the message.</param>
        /// <param name="options">The options for the send.</param>
        public virtual Task Send<T>(Action<T> messageConstructor, SendOptions options)
        {
            return Send(messageCreator.CreateInstance(messageConstructor), options);
        }

        /// <summary>
        /// Publish the message to subscribers.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        /// <param name="options">The options for the publish.</param>
        public virtual Task Publish(object message, PublishOptions options)
        {
            publishedMessages.Enqueue(new PublishedMessage<object>(message, options));
            return Task.FromResult(0);
        }

        /// <summary>
        /// Instantiates a message of type T and publishes it.
        /// </summary>
        /// <typeparam name="T">The type of message, usually an interface.</typeparam>
        /// <param name="messageConstructor">An action which initializes properties of the message.</param>
        /// <param name="publishOptions">Specific options for this event.</param>
        public virtual Task Publish<T>(Action<T> messageConstructor, PublishOptions publishOptions)
        {
            return Publish(messageCreator.CreateInstance(messageConstructor), publishOptions);
        }

        /// <summary>
        /// Sends the message to the endpoint which sent the message currently being handled.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="options">Options for this reply.</param>
        public virtual Task Reply(object message, ReplyOptions options)
        {
            repliedMessages.Enqueue(new RepliedMessage<object>(message, options));
            return Task.FromResult(0);
        }

        /// <summary>
        /// Instantiates a message of type T and performs a regular
        /// <see cref="M:NServiceBus.IMessageProcessingContext.Reply(System.Object,NServiceBus.ReplyOptions)" />.
        /// </summary>
        /// <typeparam name="T">The type of message, usually an interface.</typeparam>
        /// <param name="messageConstructor">An action which initializes properties of the message.</param>
        /// <param name="options">Options for this reply.</param>
        public virtual Task Reply<T>(Action<T> messageConstructor, ReplyOptions options)
        {
            return Reply(messageCreator.CreateInstance(messageConstructor), options);
        }

        /// <summary>
        /// Forwards the current message being handled to the destination maintaining
        /// all of its transport-level properties and headers.
        /// </summary>
        public virtual Task ForwardCurrentMessageTo(string destination)
        {
            forwardedMessages.Enqueue(destination);
            return Task.FromResult(0);
        }

        /// <summary>
        /// Moves the message being handled to the back of the list of available
        /// messages so it can be handled later.
        /// </summary>
        public virtual Task HandleCurrentMessageLater()
        {
            HandleCurrentMessageLaterWasCalled = true;
            return Task.FromResult(0);
        }

        /// <summary>
        /// Tells the endpoint to stop dispatching the current message to additional
        /// handlers.
        /// </summary>
        public virtual void DoNotContinueDispatchingCurrentMessageToHandlers()
        {
            DoNotContinueDispatchingCurrentMessageToHandlersWasCalled = true;
        }

        /// <summary>
        /// The <see cref="IMessageCreator" /> instance used to create proxy implementations for messages.
        /// </summary>
        protected IMessageCreator messageCreator;

        ConcurrentQueue<SentMessage<object>> sentMessages = new ConcurrentQueue<SentMessage<object>>();
        ConcurrentQueue<PublishedMessage<object>> publishedMessages = new ConcurrentQueue<PublishedMessage<object>>();
        ConcurrentQueue<RepliedMessage<object>> repliedMessages = new ConcurrentQueue<RepliedMessage<object>>();
        ConcurrentQueue<string> forwardedMessages = new ConcurrentQueue<string>();
    }

    /// <summary>
    /// Represents an outgoing message. Contains the message itself and it's associated options.
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    /// <typeparam name="TOptions">The options type of the message.</typeparam>
    public class OutgoingMessage<TMessage, TOptions> where TOptions : ExtendableOptions
    {
        /// <summary>
        /// Creates a new instance for the given message and options.
        /// </summary>
        protected OutgoingMessage(TMessage message, TOptions options)
        {
            Message = message;
            Options = options;
        }

        /// <summary>
        /// The outgoing message.
        /// </summary>
        public TMessage Message { get; }

        /// <summary>
        /// The options of the outgoing message.
        /// </summary>
        public TOptions Options { get; }
    }

    /// <summary>
    /// Represents an outgoing message. Contains the message itself and it's associated options.
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    public class SentMessage<TMessage> : OutgoingMessage<TMessage, SendOptions>
    {
        /// <summary>
        /// Creates a new instance for the given message and options.
        /// </summary>
        public SentMessage(TMessage message, SendOptions options) : base(message, options)
        {
        }
    }

    /// <summary>
    /// Represents an outgoing message. Contains the message itself and it's associated options.
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    public class PublishedMessage<TMessage> : OutgoingMessage<TMessage, PublishOptions>
    {
        /// <summary>
        /// Creates a new instance for the given message and options.
        /// </summary>
        public PublishedMessage(TMessage message, PublishOptions options) : base(message, options)
        {
        }
    }

    /// <summary>
    /// Represents an outgoing message. Contains the message itself and it's associated options.
    /// </summary>
    /// <typeparam name="TMessage">The message type.</typeparam>
    public class RepliedMessage<TMessage> : OutgoingMessage<TMessage, ReplyOptions>
    {
        /// <summary>
        /// Creates a new instance for the given message and options.
        /// </summary>
        public RepliedMessage(TMessage message, ReplyOptions options) : base(message, options)
        {
        }
    }
}