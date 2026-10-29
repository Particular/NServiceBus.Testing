namespace NServiceBus.Testing;

using System;
using System.Collections.Generic;
using Pipeline;
using Transport;

/// <summary>
/// A testable implementation of <see cref="IIncomingPhysicalMessageContext" />.
/// </summary>
public partial class TestableIncomingPhysicalMessageContext : TestableIncomingContext, IIncomingPhysicalMessageContext
{
    /// <summary>
    /// Creates a new instance of <see cref="TestableIncomingPhysicalMessageContext"/>.
    /// </summary>
    public TestableIncomingPhysicalMessageContext() => Message = new IncomingMessage(DefaultMessageHeaders.MessageId, base.IncomingMessageHeaders, Array.Empty<byte>());

    /// <summary>
    /// Updates the message with the given body.
    /// </summary>
    public virtual void UpdateMessage(ReadOnlyMemory<byte> body) => Message = new IncomingMessage(Message.MessageId, Message.Headers, body);

    /// <summary>
    /// The physical message being processed.
    /// </summary>
    public IncomingMessage Message { get; set; }

    internal override Dictionary<string, string> IncomingMessageHeaders
    {
        get => Message.Headers;
        set => Message = new IncomingMessage(Message.NativeMessageId, value, Message.Body, Message.ReceiveProperties);
    }
}