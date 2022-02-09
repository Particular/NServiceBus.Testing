namespace NServiceBus.Testing
{
    using System.Linq;

    /// <summary>
    /// Convenience methods to make it easy to find outgoing messages in testable contexts.
    /// </summary>
    public static class TestingExtensions
    {
        /// <summary>
        /// Find the first published message of a given type. Returns null if no messages of that type are found.
        /// </summary>
        public static TMessageType FindPublishedMessage<TMessageType>(this TestablePipelineContext context)
        {
            return (TMessageType)context.PublishedMessages.FirstOrDefault(msg => msg.Message.GetType() == typeof(TMessageType))?.Message;
        }

        /// <summary>
        /// Find the first sent message of a given type. Returns null if no messages of that type are found.
        /// </summary>
        public static TMessageType FindSentMessage<TMessageType>(this TestablePipelineContext context)
        {
            return (TMessageType)context.SentMessages.FirstOrDefault(msg => msg.Message.GetType() == typeof(TMessageType))?.Message;
        }

        /// <summary>
        /// Find the first timeout message of a given type. Returns null if no messages of that type are found.
        /// </summary>
        public static TMessageType FindTimeoutMessage<TMessageType>(this TestablePipelineContext context)
        {
            return (TMessageType)context.TimeoutMessages.FirstOrDefault(msg => msg.Message.GetType() == typeof(TMessageType))?.Message;
        }

        /// <summary>
        /// Find the first reply message of a given type. Returns null if no messages of that type are found.
        /// </summary>
        public static TMessageType FindReplyMessage<TMessageType>(this TestableMessageProcessingContext context)
        {
            return (TMessageType)context.RepliedMessages.FirstOrDefault(msg => msg.Message.GetType() == typeof(TMessageType))?.Message;
        }
    }
}
