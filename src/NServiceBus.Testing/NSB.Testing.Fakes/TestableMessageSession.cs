namespace NServiceBus.Testing
{
    using System;
    using System.Collections.Concurrent;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A testable <see cref="IMessageSession"/> implementation.
    /// </summary>
    public partial class TestableMessageSession : TestablePipelineContext, IMessageSession
    {
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
        public virtual Task Subscribe(Type eventType, SubscribeOptions options, CancellationToken cancellationToken)
        {
            subscriptions.Enqueue(new Subscription(eventType, options));
            return Task.FromResult(0);
        }

        /// <summary>
        /// Unsubscribes to receive published messages of the specified type.
        /// </summary>
        /// <param name="eventType">The type of event to unsubscribe to.</param>
        /// <param name="options">Options for the subscribe.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        public virtual Task Unsubscribe(Type eventType, UnsubscribeOptions options, CancellationToken cancellationToken)
        {
            unsubscriptions.Enqueue(new Unsubscription(eventType, options));
            return Task.FromResult(0);
        }

        /// <summary>
        /// Sends the provided message.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="sendOptions">The options for the send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        public Task Send(object message, SendOptions sendOptions, CancellationToken cancellationToken)
        {
            return Send(message, sendOptions);
        }

        /// <summary>
        /// Instantiates a message of type T and sends it.
        /// </summary>
        /// <typeparam name="T">The type of message, usually an interface.</typeparam>
        /// <param name="messageConstructor">An action which initializes properties of the message.</param>
        /// <param name="sendOptions">The options for the send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        public Task Send<T>(Action<T> messageConstructor, SendOptions sendOptions, CancellationToken cancellationToken)
        {
            return Send(messageConstructor, sendOptions);
        }

        /// <summary>
        /// Publish the message to subscribers.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        /// <param name="publishOptions">The options for the publish.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        public Task Publish(object message, PublishOptions publishOptions, CancellationToken cancellationToken)
        {
            return Publish(message, publishOptions);
        }

        /// <summary>
        /// Instantiates a message of type T and publishes it.
        /// </summary>
        /// <typeparam name="T">The type of message, usually an interface.</typeparam>
        /// <param name="messageConstructor">An action which initializes properties of the message.</param>
        /// <param name="publishOptions">Specific options for this event.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
        public Task Publish<T>(Action<T> messageConstructor, PublishOptions publishOptions, CancellationToken cancellationToken)
        {
            return Publish(messageConstructor, publishOptions);
        }

        ConcurrentQueue<Subscription> subscriptions = new ConcurrentQueue<Subscription>();

        ConcurrentQueue<Unsubscription> unsubscriptions = new ConcurrentQueue<Unsubscription>();
    }
}