namespace NServiceBus.Testing
{
    using System.Linq;

    /// <summary>
    /// Convenience methods to make it easy to find outgoing messages in testable contexts.
    /// </summary>
    public static class TestingExtensions
    {
        /// <summary>
        /// Returns the first published message of a given type,
        /// or a default value if there is no published message of the given type.
        /// </summary>
        public static TMessage FindPublishedMessage<TMessage>(this TestablePipelineContext context) =>
            (TMessage)context.PublishedMessages.FirstOrDefault(msg => msg.Message is TMessage)?.Message;

        /// <summary>
        /// Returns the first sent message of a given type,
        /// or a default value if there is no sent message of the given type.
        /// </summary>
        public static TMessage FindSentMessage<TMessage>(this TestablePipelineContext context) =>
            (TMessage)context.SentMessages.FirstOrDefault(msg => msg.Message is TMessage)?.Message;

        /// <summary>
        /// Returns the first timeout message of a given type,
        /// or a default value if there is no timeout message of the given type.
        /// </summary>
        public static TMessage FindTimeoutMessage<TMessage>(this TestablePipelineContext context) =>
            (TMessage)context.TimeoutMessages.FirstOrDefault(msg => msg.Message is TMessage)?.Message;

        /// <summary>
        /// Returns the first replied message of a given type,
        /// or a default value if there is no replied message of the given type.
        /// </summary>
        public static TMessage FindReplyMessage<TMessage>(this TestableMessageProcessingContext context) =>
            (TMessage)context.RepliedMessages.FirstOrDefault(msg => msg.Message is TMessage)?.Message;
    }
}