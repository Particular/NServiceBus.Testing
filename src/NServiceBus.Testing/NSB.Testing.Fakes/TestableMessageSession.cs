namespace NServiceBus.Testing
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;
    using NServiceBus.MessageInterfaces.MessageMapper.Reflection;

    /// <summary>
    /// A testable <see cref="IMessageSession"/> implementation.
    /// </summary>
    public partial class TestableMessageSession : IMessageSession
    {
        /// <summary>
        /// Creates a new <see cref="TestableMessageSession" /> instance.
        /// </summary>
        /// <param name="messageCreator"></param>
        public TestableMessageSession(IMessageCreator messageCreator = null)
        {
            this.messageCreator = messageCreator ?? new MessageMapper();
        }

        /// <summary>
        /// A list of all messages sent with a saga timeout header.
        /// </summary>
        public TimeoutMessage<object>[] TimeoutMessages => timeoutMessages.ToArray();

        /// <summary>
        /// A list of all messages sent by <see cref="IPipelineContext.Send" />.
        /// </summary>
        public virtual SentMessage<object>[] SentMessages => sentMessages.ToArray();

        /// <summary>
        /// A list of all messages published by <see cref="IPipelineContext.Publish" />,
        /// </summary>
        public virtual PublishedMessage<object>[] PublishedMessages => publishedMessages.ToArray();

        /// <summary>
        /// A list of all event subscriptions made from this session.
        /// </summary>
        public virtual Subscription[] Subscriptions => subscriptions.ToArray();

        /// <summary>
        /// A list of all event subscriptions canceled from this session.
        /// </summary>
        public virtual Unsubscription[] Unsubscription => unsubscriptions.ToArray();

        /// <summary>
        /// Subscribes to receive published messages of the specified type.
        /// This method is only necessary if you turned off auto-subscribe.
        /// </summary>
        /// <param name="eventType">The type of event to subscribe to.</param>
        /// <param name="options">Options for the subscribe.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        public virtual Task Subscribe(Type eventType, SubscribeOptions options, CancellationToken cancellationToken = default)
        {
            subscriptions.Enqueue(new Subscription(eventType, options));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Unsubscribes to receive published messages of the specified type.
        /// </summary>
        /// <param name="eventType">The type of event to unsubscribe to.</param>
        /// <param name="options">Options for the subscribe.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        public virtual Task Unsubscribe(Type eventType, UnsubscribeOptions options, CancellationToken cancellationToken = default)
        {
            unsubscriptions.Enqueue(new Unsubscription(eventType, options));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Sends the provided message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="sendOptions">The options for the send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        public virtual Task Send(object message, SendOptions sendOptions, CancellationToken cancellationToken = default)
        {
            var headers = sendOptions.GetHeaders();

            if (headers.ContainsKey(Headers.IsSagaTimeoutMessage))
            {
                timeoutMessages.Enqueue(GetTimeoutMessage(message, sendOptions));
            }

            sentMessages.Enqueue(new SentMessage<object>(message, sendOptions));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Instantiates a message of type T and sends it.
        /// </summary>
        /// <typeparam name="T">The type of message, usually an interface.</typeparam>
        /// <param name="messageConstructor">An action which initializes properties of the message.</param>
        /// <param name="sendOptions">The options for the send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        public virtual Task Send<T>(Action<T> messageConstructor, SendOptions sendOptions, CancellationToken cancellationToken = default) =>
            Send(messageCreator.CreateInstance(messageConstructor), sendOptions, cancellationToken);

        /// <summary>
        /// Publish the message to subscribers.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        /// <param name="publishOptions">The options for the publish.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        public virtual Task Publish(object message, PublishOptions publishOptions, CancellationToken cancellationToken = default)
        {
            publishedMessages.Enqueue(new PublishedMessage<object>(message, publishOptions));
            return Task.CompletedTask;
        }

        /// <summary>
        /// Instantiates a message of type T and publishes it.
        /// </summary>
        /// <typeparam name="T">The type of message, usually an interface.</typeparam>
        /// <param name="messageConstructor">An action which initializes properties of the message.</param>
        /// <param name="publishOptions">Specific options for this event.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        public virtual Task Publish<T>(Action<T> messageConstructor, PublishOptions publishOptions, CancellationToken cancellationToken = default) =>
            Publish(messageCreator.CreateInstance(messageConstructor), publishOptions, cancellationToken);

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

        /// <summary>
        /// Resets the the stored collections of <see cref="SentMessages"/>, <see cref="PublishedMessages"/>, <see cref="TimeoutMessages"/>,
        /// <see cref="Subscriptions"/>, and <see cref="Unsubscription"/> requests so that the testable session can be reused.
        /// </summary>
        public virtual void Reset()
        {
            sentMessages.Clear();
            publishedMessages.Clear();
            timeoutMessages.Clear();
            subscriptions.Clear();
            unsubscriptions.Clear();
        }

        /// <summary>
        /// the <see cref="IMessageCreator" /> instance used to create proxy implementation for message interfaces.
        /// </summary>
        protected IMessageCreator messageCreator;

        readonly ConcurrentQueue<Subscription> subscriptions = new ConcurrentQueue<Subscription>();
        readonly ConcurrentQueue<Unsubscription> unsubscriptions = new ConcurrentQueue<Unsubscription>();
        readonly ConcurrentQueue<PublishedMessage<object>> publishedMessages = new ConcurrentQueue<PublishedMessage<object>>();
        readonly ConcurrentQueue<SentMessage<object>> sentMessages = new ConcurrentQueue<SentMessage<object>>();
        readonly ConcurrentQueue<TimeoutMessage<object>> timeoutMessages = new ConcurrentQueue<TimeoutMessage<object>>();
    }
}
