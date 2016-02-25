namespace NServiceBus.Testing
{
    using System.Collections.Generic;
    using System.Linq;

    internal static class InvokedMessageExtensions
    {
        /// <summary>
        /// Returns all <see cref="InvokedMessage"/> instances which contain a message of type TMessage.
        /// </summary>
        public static List<InvokedMessage> Containing<TMessage>(this IEnumerable<InvokedMessage> invokedMessages)
        {
            return invokedMessages.Where(x => x.Message is TMessage).ToList();
        }
    }
}