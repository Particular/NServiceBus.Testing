namespace NServiceBus.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Represents a queued message waiting to be processed by a saga.
    /// Use <see cref="TestableSaga{TSaga,TSagaData}.HandleQueuedMessage"/> to process the next message in the queue.
    /// </summary>
    [DebuggerDisplay("QueuedSagaMessage: {Message}")]
    public class QueuedSagaMessage
    {
        /// <summary>
        /// The type of the message.
        /// </summary>
        public Type Type { get; private set; }

        /// <summary>
        /// The message to be processed.
        /// </summary>
        public object Message { get; private set; }

        /// <summary>
        /// Headers that accompany the message.
        /// </summary>
        public IReadOnlyDictionary<string, string> Headers { get; private set; }

        internal QueuedSagaMessage(Type type, object message, IReadOnlyDictionary<string, string> headers = null, Guid? autoCorrelatedSagaId = null)
        {
            Type = type;
            Message = message;

            if (autoCorrelatedSagaId.HasValue)
            {
                var editable = headers as Dictionary<string, string> ?? headers?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? [];
                editable[NServiceBus.Headers.SagaId] = autoCorrelatedSagaId.Value.ToString();
                Headers = editable;
            }
            else
            {
                Headers = headers ?? new Dictionary<string, string>();
            }
        }
    }
}
