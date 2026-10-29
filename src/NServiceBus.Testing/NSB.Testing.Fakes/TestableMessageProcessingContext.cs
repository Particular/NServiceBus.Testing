namespace NServiceBus.Testing;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

/// <summary>
/// Testable implementation of <see cref="IMessageProcessingContext" />.
/// </summary>
public partial class TestableMessageProcessingContext : TestablePipelineContext, IMessageProcessingContext
{
    /// <summary>
    /// Creates a new instance of <see cref="TestableMessageProcessingContext" /> with deterministic default message headers.
    /// </summary>
    public TestableMessageProcessingContext(IMessageCreator messageCreator = null) : base(messageCreator)
    {
    }

    /// <summary>
    /// A list of all messages sent by <see cref="IMessageProcessingContext.Reply" />.
    /// </summary>
    public virtual RepliedMessage<object>[] RepliedMessages => repliedMessages.ToArray();

    /// <summary>
    /// A list of all forwarding destinations set by <see cref="IMessageProcessingContext.ForwardCurrentMessageTo" />.
    /// </summary>
    public virtual string[] ForwardedMessages => forwardedMessages.ToArray();

    /// <summary>
    /// Gets or replaces the message headers. Dictionary implementations other than
    /// <see cref="Dictionary{TKey,TValue}"/> are copied.
    /// </summary>
    public IDictionary<string, string> MessageHeaders
    {
        get => IncomingMessageHeaders;
        set
        {
            ArgumentNullException.ThrowIfNull(value);
            IncomingMessageHeaders = value as Dictionary<string, string> ?? new Dictionary<string, string>(value);
        }
    }

    /// <summary>
    /// Sends the message to the endpoint which sent the message currently being handled.
    /// </summary>
    /// <param name="message">The message to send.</param>
    /// <param name="options">Options for this reply.</param>
    public virtual Task Reply(object message, ReplyOptions options)
    {
        repliedMessages.Enqueue(new RepliedMessage<object>(message, options));
        return Task.CompletedTask;
    }

    /// <summary>
    /// Instantiates a message of type T and performs a regular
    /// <see cref="M:NServiceBus.IMessageProcessingContext.Reply(System.Object,NServiceBus.ReplyOptions)" />.
    /// </summary>
    /// <typeparam name="T">The type of message, usually an interface.</typeparam>
    /// <param name="messageConstructor">An action which initializes properties of the message.</param>
    /// <param name="options">Options for this reply.</param>
    public virtual Task Reply<T>(Action<T> messageConstructor, ReplyOptions options) => Reply(messageCreator.CreateInstance(messageConstructor), options);

    /// <summary>
    /// Forwards the current message being handled to the destination maintaining
    /// all of its transport-level properties and headers.
    /// </summary>
    public virtual Task ForwardCurrentMessageTo(string destination)
    {
        forwardedMessages.Enqueue(destination);
        return Task.CompletedTask;
    }

    /// <summary>
    /// The ID in the message headers, or <c>null</c> if the header is absent.
    /// </summary>
    public string MessageId
    {
        get => IncomingMessageHeaders.GetValueOrDefault(Headers.MessageId);
        set => IncomingMessageHeaders[Headers.MessageId] = value;
    }

    /// <summary>
    /// The reply address in the message headers, or <c>null</c> if the header is absent.
    /// </summary>
    public string ReplyToAddress
    {
        get => IncomingMessageHeaders.GetValueOrDefault(Headers.ReplyToAddress);
        set => IncomingMessageHeaders[Headers.ReplyToAddress] = value;
    }

    internal virtual Dictionary<string, string> IncomingMessageHeaders { get; set; } = DefaultMessageHeaders.Create();

    IReadOnlyDictionary<string, string> IMessageProcessingContext.MessageHeaders => new ReadOnlyDictionary<string, string>(MessageHeaders);
    ConcurrentQueue<string> forwardedMessages = new ConcurrentQueue<string>();

    ConcurrentQueue<RepliedMessage<object>> repliedMessages = new ConcurrentQueue<RepliedMessage<object>>();
}