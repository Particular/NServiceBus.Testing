namespace NServiceBus.Testing
{
    using System.Collections.Generic;
    using System.Linq;

    internal static class InvokedMessageExtensions
    {
        public static List<RepliedMessage<TMessage>> Containing<TMessage>(this IEnumerable<RepliedMessage<object>> repliedMessages)
        {
            return repliedMessages.Where(x => x.Message is TMessage).Select(x => new RepliedMessage<TMessage>((TMessage)x.Message, x.Options)).ToList();
        }

        public static List<PublishedMessage<TMessage>> Containing<TMessage>(this IEnumerable<PublishedMessage<object>> publishedMessages)
        {
            return publishedMessages.Where(x => x.Message is TMessage).Select(x => new PublishedMessage<TMessage>((TMessage) x.Message, x.Options)).ToList();
        }

        public static List<SentMessage<TMessage>> Containing<TMessage>(this IEnumerable<SentMessage<object>> sentMessages)
        {
            return sentMessages.Where(x => x.Message is TMessage).Select(x => new SentMessage<TMessage>((TMessage) x.Message, x.Options)).ToList();
        }

        public static List<TMessage> GetMessages<TMessage>(this IEnumerable<RepliedMessage<object>> repliedMessages)
        {
            return repliedMessages.Select(x => x.Message).OfType<TMessage>().ToList();
        }

        public static List<TMessage> GetMessages<TMessage>(this IEnumerable<SentMessage<object>> sentMessages)
        {
            return sentMessages.Select(x => x.Message).OfType<TMessage>().ToList();
        }

        public static List<TMessage> GetMessages<TMessage>(this IEnumerable<PublishedMessage<object>> publishedMessage)
        {
            return publishedMessage.Select(x => x.Message).OfType<TMessage>().ToList();
        }
    }
}