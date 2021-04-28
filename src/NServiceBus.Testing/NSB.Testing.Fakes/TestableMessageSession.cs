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
#pragma warning disable PS0002 // Instance methods on types implementing ICancellableContext should not have a CancellationToken parameter
        public virtual Task Subscribe(Type eventType, SubscribeOptions options, CancellationToken cancellationToken = default)
#pragma warning restore PS0002 // Instance methods on types implementing ICancellableContext should not have a CancellationToken parameter
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
#pragma warning disable PS0002 // Instance methods on types implementing ICancellableContext should not have a CancellationToken parameter
        public virtual Task Unsubscribe(Type eventType, UnsubscribeOptions options, CancellationToken cancellationToken = default)
#pragma warning restore PS0002 // Instance methods on types implementing ICancellableContext should not have a CancellationToken parameter
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
#pragma warning disable PS0002 // Instance methods on types implementing ICancellableContext should not have a CancellationToken parameter
        public Task Send(object message, SendOptions sendOptions, CancellationToken cancellationToken = default) =>
#pragma warning restore PS0002 // Instance methods on types implementing ICancellableContext should not have a CancellationToken parameter
            Send(message, sendOptions, cancellationToken);

        /// <summary>
        /// Instantiates a message of type T and sends it.
        /// </summary>
        /// <typeparam name="T">The type of message, usually an interface.</typeparam>
        /// <param name="messageConstructor">An action which initializes properties of the message.</param>
        /// <param name="sendOptions">The options for the send.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
#pragma warning disable PS0002 // Instance methods on types implementing ICancellableContext should not have a CancellationToken parameter
        public Task Send<T>(Action<T> messageConstructor, SendOptions sendOptions, CancellationToken cancellationToken = default) =>
#pragma warning restore PS0002 // Instance methods on types implementing ICancellableContext should not have a CancellationToken parameter
            Send(messageConstructor, sendOptions, cancellationToken);

        /// <summary>
        /// Publish the message to subscribers.
        /// </summary>
        /// <param name="message">The message to publish.</param>
        /// <param name="publishOptions">The options for the publish.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
#pragma warning disable PS0002 // Instance methods on types implementing ICancellableContext should not have a CancellationToken parameter
        public Task Publish(object message, PublishOptions publishOptions, CancellationToken cancellationToken = default) =>
#pragma warning restore PS0002 // Instance methods on types implementing ICancellableContext should not have a CancellationToken parameter
            Publish(message, publishOptions, cancellationToken);

        /// <summary>
        /// Instantiates a message of type T and publishes it.
        /// </summary>
        /// <typeparam name="T">The type of message, usually an interface.</typeparam>
        /// <param name="messageConstructor">An action which initializes properties of the message.</param>
        /// <param name="publishOptions">Specific options for this event.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> to observe.</param>
#pragma warning disable PS0002 // Instance methods on types implementing ICancellableContext should not have a CancellationToken parameter
        public Task Publish<T>(Action<T> messageConstructor, PublishOptions publishOptions, CancellationToken cancellationToken = default) =>
#pragma warning restore PS0002 // Instance methods on types implementing ICancellableContext should not have a CancellationToken parameter
            Publish(messageConstructor, publishOptions, cancellationToken);

        ConcurrentQueue<Subscription> subscriptions = new ConcurrentQueue<Subscription>();

        ConcurrentQueue<Unsubscription> unsubscriptions = new ConcurrentQueue<Unsubscription>();
    }
}