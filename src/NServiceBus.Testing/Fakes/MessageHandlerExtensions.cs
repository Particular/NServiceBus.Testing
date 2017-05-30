namespace NServiceBus
{
    using System.Threading.Tasks;
    using NServiceBus.Testing;

    /// <summary>
    /// Extension methods for message handlers.
    /// </summary>
    public static class MessageHandlerExtensions
    {
        /// <summary>
        /// Invokes the handler method with the given message and a new <see cref="TestableMessageHandlerContext" />.
        /// </summary>
        /// <param name="handler">The handler to invoke.</param>
        /// <param name="message">The message to pass to the handler.</param>
        /// <returns>The created <see cref="TestableMessageHandlerContext" /> which was passed to the handler.</returns>
        public static async Task<TestableMessageHandlerContext> Handle<T>(this IHandleMessages<T> handler, T message)
        {
            var context = new TestableMessageHandlerContext();
            await handler.Handle(message, context);
            return context;
        }

        /// <summary>
        /// Invokes the saga method with the given message and a new <see cref="TestableMessageHandlerContext" />.
        /// </summary>
        /// <param name="saga">The saga to invoke.</param>
        /// <param name="message">The message to pass to the saga.</param>
        /// <returns>The created <see cref="TestableMessageHandlerContext" /> which was passed to the saga.</returns>
        public static async Task<TestableMessageHandlerContext> Handle<T>(this IAmStartedByMessages<T> saga, T message)
        {
            var context = new TestableMessageHandlerContext();
            await saga.Handle(message, context);
            return context;
        }
    }
}