namespace NServiceBus.Testing
{
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Extension methods for easier type safe access to instances of <see cref="OutgoingMessage{TMessage,TOptions}" />
    /// </summary>
    public static class TimeoutMessageExtensions
    {
        internal static IEnumerable<TimeoutMessage<TMessage>> Containing<TMessage>(this IEnumerable<TimeoutMessage<object>> timeoutMessages)
        {
            return timeoutMessages
                .Where(x => x.Message is TMessage)
                .Select(x => new TimeoutMessage<TMessage>((TMessage)x.Message, x.Options, x.Within));
        }
    }
}