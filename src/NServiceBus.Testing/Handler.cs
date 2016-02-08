namespace NServiceBus.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using NServiceBus.MessageInterfaces.MessageMapper.Reflection;
    using NServiceBus.Testing.ExpectedInvocations;

    /// <summary>
    /// Message handler unit testing framework.
    /// </summary>
    public class Handler<T>
    {
        private readonly T handler;

        IMessageCreator messageCreator = new MessageMapper();

        TestableMessageHandlerContext testableMessageHandlerContext;

        /// <summary>
        /// Creates a new instance of the handler tester.
        /// </summary>
        internal Handler(T handler)
        {
            this.handler = handler;
            testableMessageHandlerContext = new TestableMessageHandlerContext(messageCreator);
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
        /// Set the headers on an incoming message that will be return
        /// when code calls Bus.CurrentMessageContext.Headers
        /// </summary>
        public Handler<T> SetIncomingHeader(string key, string value)
        {
            testableMessageHandlerContext.IncomingHeaders[key] = value;

            return this;
        }

        /// <summary>
        /// Check that the handler sends a message of the given type complying with the given predicate.
        /// </summary>
        public Handler<T> ExpectSend<TMessage>(Func<TMessage, bool> check)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectSend<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the handler does not send a message of the given type complying with the given predicate.
        /// </summary>
        public Handler<T> ExpectNotSend<TMessage>(Func<TMessage, bool> check)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectNotSend<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the handler does not reply with a given message
        /// </summary>
        public Handler<T> ExpectNotReply<TMessage>(Func<TMessage, bool> check)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectNotReply<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the handler replies with the given message type complying with the given predicate.
        /// </summary>
        public Handler<T> ExpectReply<TMessage>(Func<TMessage, bool> check)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectReply<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the handler sends the given message type to its local queue
        /// and that the message complies with the given predicate.
        /// </summary>
        public Handler<T> ExpectSendLocal<TMessage>(Func<TMessage, bool> check)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectSendLocal<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the handler does not send a message type to its local queue that complies with the given predicate.
        /// </summary>
        public Handler<T> ExpectNotSendLocal<TMessage>(Func<TMessage, bool> check)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectNotSendLocal<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the handler uses the bus to return the appropriate error code.
        /// </summary>
        public Handler<T> ExpectReturn<TEnum>(Func<TEnum, bool> check)
        {
            //testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectedReturnInvocation<TEnum> { Check = check });
            return this;
        }

        /// <summary>
        /// Check that the handler publishes a message of the given type complying with the given predicate.
        /// </summary>
        public Handler<T> ExpectPublish<TMessage>(Func<TMessage, bool> check)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectPublish<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the handler does not publish any messages of the given type complying with the given predicate.
        /// </summary>
        public Handler<T> ExpectNotPublish<TMessage>(Func<TMessage, bool> check)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectNotPublish<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the handler tells the bus to stop processing the current message.
        /// </summary>
        public Handler<T> ExpectDoNotContinueDispatchingCurrentMessageToHandlers()
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectDoNotContinueDispatching());
            return this;
        }

        /// <summary>
        /// Check that the handler tells the bus to handle the current message later.
        /// </summary>
        public Handler<T> ExpectHandleCurrentMessageLater()
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectHandleCurrentMessageLater());
            return this;
        }

        /// <summary>
        /// Check that the handler defers a message of the given type.
        /// </summary>
        public Handler<T> ExpectDefer<TMessage>(Func<TMessage, TimeSpan, bool> check)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectDelayDeliveryWith<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the handler defers a message of the given type.
        /// </summary>
        public Handler<T> ExpectDefer<TMessage>(Func<TMessage, DateTime, bool> check)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectDoNotDeliverBefore<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the handler doesn't defer a message of the given type.
        /// </summary>
        public Handler<T> ExpectNotDefer<TMessage>(Func<TMessage, TimeSpan, bool> check)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectNotDelayDeliveryWith<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the handler doesn't defer a message of the given type.
        /// </summary>
        public Handler<T> ExpectNotDefer<TMessage>(Func<TMessage, DateTime, bool> check)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectNotDoNotDeliverBefore<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the handler doesn't forward a message to the given destination.
        /// </summary>
        public Handler<T> ExpectNotForwardCurrentMessageTo(Func<string, bool> check)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectNotForwardCurrentMessageTo(check));
            return this;
        }

        /// <summary>
        /// Check that the handler forwards a message to the given destination.
        /// </summary>
        public Handler<T> ExpectForwardCurrentMessageTo(Func<string, bool> check)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectForwardCurrentMessageTo(check));
            return this;
        }

        /// <summary>
        /// Activates the test that has been set up passing in the given message.
        /// </summary>
        public void OnMessage<TMessage>(Action<TMessage> initializeMessage = null)
        {
            OnMessage(Guid.NewGuid().ToString("N"), initializeMessage);
        }

        /// <summary>
        /// Activates the test that has been set up passing in the given message, 
        /// setting the incoming headers and the message Id.
        /// </summary>
        public void OnMessage<TMessage>(string messageId, Action<TMessage> initializeMessage = null)
        {
            var message = messageCreator.CreateInstance<TMessage>();
            OnMessage(message, messageId);
        }

        /// <summary>Activates the test that has been set up passing in a specific message to be used.</summary>
        /// <param name="initializedMessage">A message to be used with message handler.</param>
        /// <remarks>This is different from "<![CDATA[public void OnMessage<TMessage>(Action<TMessage> initializedMessage)]]>" in a way that it uses the message, and not calls to an action.</remarks>
        /// <example><![CDATA[var message = new TestMessage {//...}; Test.Handler<EmptyHandler>().OnMessage<TestMessage>(message);]]></example>
        public void OnMessage<TMessage>(TMessage initializedMessage)
        {
            OnMessage(initializedMessage, Guid.NewGuid().ToString("N"));
        }

        /// <summary>
        /// Activates the test that has been set up passing in given message,
        /// setting the incoming headers and the message Id.
        /// </summary>
        public void OnMessage<TMessage>(TMessage message, string messageId)
        {
            testableMessageHandlerContext.MessageId = messageId;

            var h = GetMessageHandler<TMessage>(handler.GetType(), message.GetType());
            h.Invoke(handler, new object[]
            {
                message,
                testableMessageHandlerContext
            });

            testableMessageHandlerContext.Validate();
        }

        /// <summary>
        /// Check that the handler sends the given message type to the appropriate destination.
        /// </summary>
        [ObsoleteEx(
            RemoveInVersion = "7",
            TreatAsErrorFromVersion = "6")]
        public Handler<T> ExpectSendToDestination<TMessage>(Func<TMessage, string, bool> check)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Check that the handler sends a message of the given type to sites.
        /// </summary>
        [ObsoleteEx(
            RemoveInVersion = "7",
            TreatAsErrorFromVersion = "6")]
        public Handler<T> ExpectSendToSites<TMessage>(Func<TMessage, IEnumerable<string>, bool> check)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Check that the handler doesn't send a message of the given type to sites.		
        /// </summary>
        [ObsoleteEx(
            RemoveInVersion = "7",
            TreatAsErrorFromVersion = "6")]
        public Handler<T> ExpectNotSendToSites<TMessage>(Func<TMessage, IEnumerable<string>, bool> check)
        {
            throw new NotImplementedException();
        }

        private static MethodInfo GetMessageHandler<TMessage>(Type targetType, Type messageType)
        {
            var method = targetType.GetMethod("Handle", new[]
            {
                messageType,
                typeof(IMessageHandlerContext)
            });
            if (method != null)
            {
                return method;
            }

            var realMessageType = typeof(TMessage).IsInterface ? typeof(TMessage) : messageType;
            var handlerType = typeof(IHandleMessages<>).MakeGenericType(realMessageType);
            return targetType.GetInterfaceMap(handlerType)
                .TargetMethods
                .FirstOrDefault();
        }
    }
}