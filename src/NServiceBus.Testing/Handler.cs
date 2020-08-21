namespace NServiceBus.Testing
{
    using System;
    using System.Threading.Tasks;
    using MessageInterfaces.MessageMapper.Reflection;

    /// <summary>
    /// Message handler unit testing framework.
    /// </summary>
    public class Handler<T>
    {
        /// <summary>
        /// Creates a new instance of the handler tester.
        /// </summary>
        internal Handler(T handler)
        {
            this.handler = handler;
            testableMessageHandlerContext = new TestingContext(messageCreator);
        }

        /// <summary>
        /// Provides a way to set external dependencies on the handler under test.
        /// </summary>
        public Handler<T> WithExternalDependencies(Action<T> actionToSetUpExternalDependencies)
        {
            actionToSetUpExternalDependencies(handler);

            return this;
        }

        /// <summary>
        /// Provides a way to customize the <see cref="IMessageHandlerContext" /> instance received by the message handler.
        /// </summary>
        public Handler<T> ConfigureHandlerContext(Action<TestableMessageHandlerContext> contextInitializer)
        {
            contextInitializer(testableMessageHandlerContext);

            return this;
        }

        /// <summary>
        /// Set the headers on an incoming message that will be return
        /// when code calls Bus.CurrentMessageContext.Headers
        /// </summary>
        public Handler<T> SetIncomingHeader(string key, string value)
        {
            testableMessageHandlerContext.MessageHeaders[key] = value;

            return this;
        }

        /// <summary>
        /// Check that the handler sends a message of the given type complying with the given predicate.
        /// </summary>
        public Handler<T> ExpectSend<TMessage>(Func<TMessage, SendOptions, bool> check = null)
        {
            testableMessageHandlerContext.AddExpectation(new ExpectSend<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the handler sends a message of the given type complying with the given predicate.
        /// </summary>
        public Handler<T> ExpectSend<TMessage>(Func<TMessage, bool> check)
        {
            return ExpectSend((TMessage m, SendOptions _) => check(m));
        }

        /// <summary>
        /// Check that the handler does not send a message of the given type complying with the given predicate.
        /// </summary>
        public Handler<T> ExpectNotSend<TMessage>(Func<TMessage, SendOptions, bool> check = null)
        {
            testableMessageHandlerContext.AddExpectation(new ExpectNotSend<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the handler does not send a message of the given type complying with the given predicate.
        /// </summary>
        public Handler<T> ExpectNotSend<TMessage>(Func<TMessage, bool> check)
        {
            return ExpectNotSend((TMessage m, SendOptions _) => check(m));
        }

        /// <summary>
        /// Check that the handler does not reply with a given message
        /// </summary>
        public Handler<T> ExpectNotReply<TMessage>(Func<TMessage, ReplyOptions, bool> check = null)
        {
            testableMessageHandlerContext.AddExpectation(new ExpectNotReply<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the handler does not reply with a given message
        /// </summary>
        public Handler<T> ExpectNotReply<TMessage>(Func<TMessage, bool> check)
        {
            return ExpectNotReply((TMessage m, ReplyOptions _) => check(m));
        }

        /// <summary>
        /// Check that the handler replies with the given message type complying with the given predicate.
        /// </summary>
        public Handler<T> ExpectReply<TMessage>(Func<TMessage, ReplyOptions, bool> check = null)
        {
            testableMessageHandlerContext.AddExpectation(new ExpectReply<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the handler replies with the given message type complying with the given predicate.
        /// </summary>
        public Handler<T> ExpectReply<TMessage>(Func<TMessage, bool> check)
        {
            return ExpectReply((TMessage m, ReplyOptions _) => check(m));
        }

        /// <summary>
        /// Check that the handler sends the given message type to its local queue
        /// and that the message complies with the given predicate.
        /// </summary>
        public Handler<T> ExpectSendLocal<TMessage>(Func<TMessage, bool> check = null)
        {
            testableMessageHandlerContext.AddExpectation(new ExpectSendLocal<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the handler does not send a message type to its local queue that complies with the given predicate.
        /// </summary>
        public Handler<T> ExpectNotSendLocal<TMessage>(Func<TMessage, bool> check = null)
        {
            testableMessageHandlerContext.AddExpectation(new ExpectNotSendLocal<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the handler publishes a message of the given type complying with the given predicate.
        /// </summary>
        public Handler<T> ExpectPublish<TMessage>(Func<TMessage, PublishOptions, bool> check = null)
        {
            testableMessageHandlerContext.AddExpectation(new ExpectPublish<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the handler publishes a message of the given type complying with the given predicate.
        /// </summary>
        public Handler<T> ExpectPublish<TMessage>(Func<TMessage, bool> check)
        {
            return ExpectPublish((TMessage m, PublishOptions _) => check(m));
        }

        /// <summary>
        /// Check that the handler does not publish any messages of the given type complying with the given predicate.
        /// </summary>
        public Handler<T> ExpectNotPublish<TMessage>(Func<TMessage, PublishOptions, bool> check = null)
        {
            testableMessageHandlerContext.AddExpectation(new ExpectNotPublish<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the handler does not publish any messages of the given type complying with the given predicate.
        /// </summary>
        public Handler<T> ExpectNotPublish<TMessage>(Func<TMessage, bool> check)
        {
            return ExpectNotPublish((TMessage m, PublishOptions _) => check(m));
        }

        /// <summary>
        /// Check that the handler tells the bus to stop processing the current message.
        /// </summary>
        public Handler<T> ExpectDoNotContinueDispatchingCurrentMessageToHandlers()
        {
            testableMessageHandlerContext.AddExpectation(new ExpectDoNotContinueDispatching());
            return this;
        }


        /// <summary>
        /// Check that the handler defers a message of the given type.
        /// </summary>
        public Handler<T> ExpectDefer<TMessage>(Func<TMessage, TimeSpan, bool> check)
        {
            testableMessageHandlerContext.AddExpectation(new ExpectDelayDeliveryWith<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the handler defers a message of the given type.
        /// </summary>
        public Handler<T> ExpectDefer<TMessage>(Func<TMessage, DateTimeOffset, bool> check)
        {
            testableMessageHandlerContext.AddExpectation(new ExpectDoNotDeliverBefore<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the handler doesn't defer a message of the given type.
        /// </summary>
        public Handler<T> ExpectNotDefer<TMessage>(Func<TMessage, TimeSpan, bool> check)
        {
            testableMessageHandlerContext.AddExpectation(new ExpectNotDelayDeliveryWith<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the handler doesn't defer a message of the given type.
        /// </summary>
        public Handler<T> ExpectNotDefer<TMessage>(Func<TMessage, DateTimeOffset, bool> check)
        {
            testableMessageHandlerContext.AddExpectation(new ExpectNotDoNotDeliverBefore<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the handler doesn't forward a message to the given destination.
        /// </summary>
        public Handler<T> ExpectNotForwardCurrentMessageTo(Func<string, bool> check = null)
        {
            testableMessageHandlerContext.AddExpectation(new ExpectNotForwardCurrentMessageTo(check));
            return this;
        }

        /// <summary>
        /// Check that the handler forwards a message to the given destination.
        /// </summary>
        public Handler<T> ExpectForwardCurrentMessageTo(Func<string, bool> check = null)
        {
            testableMessageHandlerContext.AddExpectation(new ExpectForwardCurrentMessageTo(check));
            return this;
        }

        /// <summary>
        /// Check that the handler sends the given message type to the appropriate destination.
        /// </summary>
        public Handler<T> ExpectSendToDestination<TMessage>(Func<TMessage, string, bool> check)
        {
            testableMessageHandlerContext.AddExpectation(new ExpectSendToDestination<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the handler does not send the given message type to the given destination.
        /// </summary>
        public Handler<T> ExpectNotSendToDestination<TMessage>(Func<TMessage, string, bool> check = null)
        {
            testableMessageHandlerContext.AddExpectation(new ExpectNotSendToDestination<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Activates the test that has been set up passing in the given message,
        /// setting the incoming headers and the message Id.
        /// </summary>
        public void OnMessage<TMessage>(string messageId, Action<TMessage> initializeMessage = null)
        {
            testableMessageHandlerContext.MessageId = messageId;
            OnMessage(initializeMessage);
        }

        /// <summary>
        /// Activates the test that has been set up passing in the given message.
        /// </summary>
        public void OnMessage<TMessage>(Action<TMessage> initializeMessage = null)
        {
            OnMessageAsync(initializeMessage).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Activates the test that has been set up passing in given message,
        /// setting the incoming headers and the message Id.
        /// </summary>
        public void OnMessage<TMessage>(TMessage message, string messageId)
        {
            OnMessageAsync(message, messageId).GetAwaiter().GetResult();
        }

        /// <summary>Activates the test that has been set up passing in a specific message to be used.</summary>
        /// <param name="initializedMessage">A message to be used with message handler.</param>
        /// <remarks>
        /// This is different from <see cref="OnMessage{TMessage}(System.Action{TMessage})" /> in a way that
        /// it uses the message, and not calls to an action.
        /// </remarks>
        /// <example><![CDATA[var message = new TestMessage {//...}; Test.Handler<EmptyHandler>().OnMessage<TestMessage>(message);]]></example>
        public void OnMessage<TMessage>(TMessage initializedMessage)
        {
            OnMessageAsync(initializedMessage).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Activates the test that has been set up passing in the given message,
        /// setting the incoming headers and the message Id.
        /// </summary>
        public Task OnMessageAsync<TMessage>(string messageId, Action<TMessage> initializeMessage = null)
        {
            testableMessageHandlerContext.MessageId = messageId;
            return OnMessageAsync(initializeMessage);
        }

        /// <summary>
        /// Activates the test that has been set up passing in the given message.
        /// </summary>
        public Task OnMessageAsync<TMessage>(Action<TMessage> initializeMessage = null)
        {
            var message = messageCreator.CreateInstance<TMessage>();
            initializeMessage?.Invoke(message);
            return OnMessageAsync(message);
        }

        /// <summary>
        /// Activates the test that has been set up passing in given message,
        /// setting the incoming headers and the message Id.
        /// </summary>
        public Task OnMessageAsync<TMessage>(TMessage message, string messageId)
        {
            testableMessageHandlerContext.MessageId = messageId;

            return OnMessageAsync(message);
        }

        /// <summary>Activates the test that has been set up passing in a specific message to be used.</summary>
        /// <param name="initializedMessage">A message to be used with message handler.</param>
        /// <remarks>
        /// This is different from <see cref="OnMessage{TMessage}(System.Action{TMessage})" /> in a way that
        /// it uses the message, and not calls to an action.
        /// </remarks>
        /// <example><![CDATA[var message = new TestMessage {//...}; Test.Handler<EmptyHandler>().OnMessage<TestMessage>(message);]]></example>
        public async Task OnMessageAsync<TMessage>(TMessage initializedMessage)
        {
            var messageType = messageCreator.GetMappedTypeFor(initializedMessage.GetType());
            var handleMethods = handler.GetType().CreateInvokers(messageType, typeof(IHandleMessages<>));

            await handleMethods.InvokeSerially(handler, initializedMessage, testableMessageHandlerContext).ConfigureAwait(false);

            testableMessageHandlerContext.Validate();
        }

        readonly T handler;

        MessageMapper messageCreator = new MessageMapper();

        TestingContext testableMessageHandlerContext;
    }
}