namespace NServiceBus.Testing
{
    using System.Collections.Generic;
    using System.Linq;

    static class InvokedMessageExtensions
    {
        public static IEnumerable<RepliedMessage<TMessage>> Containing<TMessage>(this IEnumerable<RepliedMessage<object>> repliedMessages)
        {
            return repliedMessages
                .Where(x => x.Message is TMessage)
                .Select(x => new RepliedMessage<TMessage>((TMessage) x.Message, x.Options));
        }

        public static IEnumerable<TimeoutMessage<TMessage>> Containing<TMessage>(this IEnumerable<TimeoutMessage<object>> timeoutMessages)
        {
            return timeoutMessages
                .Where(x => x.Message is TMessage)
                .Select(x => new TimeoutMessage<TMessage>((TMessage) x.Message, x.Options, x.Within));
        }

        public static IEnumerable<PublishedMessage<TMessage>> Containing<TMessage>(this IEnumerable<PublishedMessage<object>> publishedMessages)
        {
            return publishedMessages
                .Where(x => x.Message is TMessage)
                .Select(x => new PublishedMessage<TMessage>((TMessage) x.Message, x.Options));
        }

        public static IEnumerable<SentMessage<TMessage>> Containing<TMessage>(this IEnumerable<SentMessage<object>> sentMessages)
        {
            return sentMessages
                .Where(x => x.Message is TMessage)
                .Select(x => new SentMessage<TMessage>((TMessage) x.Message, x.Options));
        }
    }
}