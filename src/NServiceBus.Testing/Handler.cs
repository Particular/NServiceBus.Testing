﻿namespace NServiceBus.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using NServiceBus.MessageInterfaces.MessageMapper.Reflection;

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
            testableMessageHandlerContext = new TestableMessageHandlerContext();
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
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectedSendInvocation<TMessage> { Check = check });
            return this;
        }

        /// <summary>
        /// Check that the handler does not send a message of the given type complying with the given predicate.
        /// </summary>
        public Handler<T> ExpectNotSend<TMessage>(Func<TMessage, bool> check)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectedNotSendInvocation<TMessage> { Check = check });
            return this;
        }

        /// <summary>
        /// Check that the handler does not reply with a given message
        /// </summary>
        public Handler<T> ExpectNotReply<TMessage>(Func<TMessage, bool> check)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectedNotReplyInvocation<TMessage> { Check = check });
            return this;
        }

        /// <summary>
        /// Check that the handler replies with the given message type complying with the given predicate.
        /// </summary>
        public Handler<T> ExpectReply<TMessage>(Func<TMessage, bool> check)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectedReplyInvocation<TMessage> { Check = check });
            return this;
        }

        /// <summary>
        /// Check that the handler sends the given message type to its local queue
        /// and that the message complies with the given predicate.
        /// </summary>
        public Handler<T> ExpectSendLocal<TMessage>(Func<TMessage, bool> check)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectedSendLocalInvocation<TMessage> { Check = check });
            return this;
        }

        /// <summary>
        /// Check that the handler does not send a message type to its local queue that complies with the given predicate.
        /// </summary>
        public Handler<T> ExpectNotSendLocal<TMessage>(Func<TMessage, bool> check)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectedNotSendLocalInvocation<TMessage> { Check = check });
            return this;
        }

        /// <summary>
        /// Check that the handler uses the bus to return the appropriate error code.
        /// </summary>
        public Handler<T> ExpectReturn<TEnum>(Func<TEnum, bool> check)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectedReturnInvocation<TEnum> { Check = check });
            return this;
        }

        /// <summary>
        /// Check that the handler sends the given message type to the appropriate destination.
        /// </summary>
        public Handler<T> ExpectSendToDestination<TMessage>(Func<TMessage, string, bool> check)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectedSendToDestinationInvocation<TMessage> { Check = check });
            return this;
        }

        /// <summary>
        /// Check that the handler publishes a message of the given type complying with the given predicate.
        /// </summary>
        public Handler<T> ExpectPublish<TMessage>(Func<TMessage, bool> check)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectedPublishInvocation<TMessage> { Check = check });
            return this;
        }

        /// <summary>
        /// Check that the handler does not publish any messages of the given type complying with the given predicate.
        /// </summary>
        public Handler<T> ExpectNotPublish<TMessage>(Func<TMessage, bool> check)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectedNotPublishInvocation<TMessage> { Check = check });
            return this;
        }

        /// <summary>
        /// Check that the handler tells the bus to stop processing the current message.
        /// </summary>
        public Handler<T> ExpectDoNotContinueDispatchingCurrentMessageToHandlers()
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectedDoNotContinueDispatchingCurrentMessageToHandlersInvocation<object>());
            return this;
        }

        /// <summary>
        /// Check that the handler tells the bus to handle the current message later.
        /// </summary>
        public Handler<T> ExpectHandleCurrentMessageLater()
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectedHandleCurrentMessageLaterInvocation<object>());
            return this;
        }

        /// <summary>
        /// Check that the handler sends a message of the given type to sites.
        /// </summary>
        public Handler<T> ExpectSendToSites<TMessage>(Func<TMessage, IEnumerable<string>, bool> check)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectedSendToSitesInvocation<TMessage> { Check = check });
            return this;
        }

        /// <summary>
        /// Check that the handler doesn't send a message of the given type to sites.
        /// </summary>
        public Handler<T> ExpectNotSendToSites<TMessage>(Func<TMessage, IEnumerable<string>, bool> check)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectedNotSendToSitesInvocation<TMessage> { Check = check });
            return this;
        }

        /// <summary>
        /// Check that the handler defers a message of the given type.
        /// </summary>
        public Handler<T> ExpectDefer<TMessage>(Func<TMessage, TimeSpan, bool> check)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectedDeferMessageInvocation<TMessage, TimeSpan> { Check = check });
            return this;
        }

        /// <summary>
        /// Check that the handler defers a message of the given type.
        /// </summary>
        public Handler<T> ExpectDefer<TMessage>(Func<TMessage, DateTime, bool> check)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectedDeferMessageInvocation<TMessage, DateTime> { Check = check });
            return this;
        }

        /// <summary>
        /// Check that the handler doesn't defer a message of the given type.
        /// </summary>
        public Handler<T> ExpectNotDefer<TMessage>(Func<TMessage, TimeSpan, bool> check)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectedNotDeferMessageInvocation<TMessage, TimeSpan> { Check = check });
            return this;
        }

        /// <summary>
        /// Check that the handler doesn't defer a message of the given type.
        /// </summary>
        public Handler<T> ExpectNotDefer<TMessage>(Func<TMessage, DateTime, bool> check)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectedNotDeferMessageInvocation<TMessage, DateTime> { Check = check });
            return this;
        }

        /// <summary>
        /// Check that the handler doesn't forward a message to the given destination.
        /// </summary>
        public Handler<T> ExpectNotForwardCurrentMessageTo(Func<string, bool> check)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectedNotForwardCurrentMessageToInvocation{ Check = check });
            return this;
        }

        /// <summary>
        /// Check that the handler forwards a message to the given destination.
        /// </summary>
        public Handler<T> ExpectForwardCurrentMessageTo(Func<string, bool> check)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectedForwardCurrentMessageToInvocation { Check = check });
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
            var h = GetMessageHandler(handler.GetType(), message.GetType());
            h.Invoke(handler, new object[] { message, testableMessageHandlerContext });

            testableMessageHandlerContext.Validate();
        }

        private static MethodInfo GetMessageHandler(Type targetType, Type messageType)
        {
            var method = targetType.GetMethod("Handle", new[] { messageType, typeof(IMessageHandlerContext) });
            if (method != null) return method;

            var handlerType = typeof(IHandleMessages<>).MakeGenericType(messageType);
            return targetType.GetInterfaceMap(handlerType)
                            .TargetMethods
                            .FirstOrDefault();
        }
    }
}
