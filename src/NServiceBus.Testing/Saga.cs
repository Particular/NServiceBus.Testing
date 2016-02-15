namespace NServiceBus.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NServiceBus.MessageInterfaces.MessageMapper.Reflection;
    using NServiceBus.Testing.ExpectedInvocations;

    /// <summary>
    /// Saga unit testing framework.
    /// </summary>
    public class Saga<T> where T : Saga, new()
    {
        readonly T saga;

        IMessageCreator messageCreator = new MessageMapper();

        TestableMessageHandlerContext testableMessageHandlerContext;

        Func<string> messageId = () => Guid.NewGuid().ToString();

        IDictionary<string, string> incomingHeaders = new Dictionary<string, string>();

        internal Saga(T saga)
        {
            this.saga = saga;
            testableMessageHandlerContext = new TestableMessageHandlerContext();

            if (saga.Entity == null)
            {
                var prop = typeof(T).GetProperty("Data");
                if (prop == null)
                {
                    return;
                }

                var sagaData = Activator.CreateInstance(prop.PropertyType) as IContainSagaData;
                saga.Entity = sagaData;
            }

            saga.Entity.OriginalMessageId = Guid.NewGuid().ToString();
            saga.Entity.Originator = "client";

        }
        /// <summary>
        /// Provides a way to set external dependencies on the saga under test.
        /// </summary>
        public Saga<T> WithExternalDependencies(Action<T> actionToSetUpExternalDependencies)
        {
            actionToSetUpExternalDependencies(saga);
            return this;
        }

        /// <summary>
        /// Set the address of the client that caused the saga to be started.
        /// </summary>
        public Saga<T> WhenReceivesMessageFrom(string client)
        {
            saga.Entity.Originator = client;
            return this;
        }

        /// <summary>
        /// Set the headers on an incoming message that will be return
        /// when code calls Bus.CurrentMessageContext.Headers
        /// </summary>
        public Saga<T> SetIncomingHeader(string key, string value)
        {
            incomingHeaders[key] = value;
            return this;
        }

        /// <summary>
        /// Sets the Id of the incoming message that will be returned
        /// when code calls Bus.CurrentMessageContext.Id
        /// </summary>
        public Saga<T> SetMessageId(string messageId)
        {
            this.messageId = () => messageId;
            return this;
        }

        /// <summary>
        /// Get the headers set by the saga when it sends a message.
        /// </summary>
        public IDictionary<string, string> OutgoingHeaders { get; set; }

        /// <summary>
        /// Check that the saga sends a message of the given type complying with the given predicate.
        /// </summary>
        public Saga<T> ExpectSend<TMessage>(Func<TMessage, bool> check = null)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectedInvocations.ExpectedSendInvocation<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the saga sends a message of the given type complying with user-supplied assertions.
        /// </summary>
        /// <param name="check">An action containing assertions on the message.</param>
        public Saga<T> ExpectSend<TMessage>(Action<TMessage> check)
        {
            return ExpectSend(CheckActionToFunc(check));
        }

        /// <summary>
        /// Check that the saga does not send a message of the given type complying with the given predicate.
        /// </summary>
        public Saga<T> ExpectNotSend<TMessage>(Func<TMessage, bool> check)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectedInvocations.ExpectedNotSendInvocation<TMessage>(check));
            return this;

        }

        /// <summary>
        /// Check that the saga does not send a message of the given type complying with the given predicate.
        /// </summary>
        /// <param name="check">An action containing assertions on the message.</param>
        public Saga<T> ExpectNotSend<TMessage>(Action<TMessage> check)
        {
            return ExpectSend(CheckActionToFunc(check));
        }

        /// <summary>
        /// Check that the saga replies with the given message type complying with the given predicate.
        /// </summary>
        public Saga<T> ExpectReply<TMessage>(Func<TMessage, bool> check = null)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectedInvocations.ExpectedReplyInvocation<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the saga sends the given message type to its local queue
        /// and that the message complies with the given predicate.
        /// </summary>
        public Saga<T> ExpectSendLocal<TMessage>(Func<TMessage, bool> check = null)
        {
            throw new NotImplementedException();

        }

        /// <summary>
        /// Check that the saga sends the given message type to its local queue
        /// and that the message complies with the given predicate.
        /// </summary>
        /// <param name="check">An action that performs assertions on the message.</param>
        public Saga<T> ExpectSendLocal<TMessage>(Action<TMessage> check)
        {
            throw new NotImplementedException();

        }

        /// <summary>
        /// Check that the saga does not send a message type to its local queue that complies with the given predicate.
        /// </summary>
        public Saga<T> ExpectNotSendLocal<TMessage>(Func<TMessage, bool> check)
        {
            throw new NotImplementedException();

        }

        /// <summary>
        /// Check that the saga does not send a message type to its local queue that complies with the given predicate.
        /// </summary>
        /// <param name="check">An action that performs assertions on the message.</param>
        public Saga<T> ExpectNotSendLocal<TMessage>(Action<TMessage> check)
        {
            throw new NotImplementedException();

        }

        /// <summary>
        /// Check that the saga sends the given message type to the appropriate destination.
        /// </summary>
        public Saga<T> ExpectSendToDestination<TMessage>(Func<TMessage, string, bool> check)
        {
            throw new NotImplementedException();

        }

        /// <summary>
        /// Check that the saga sends the given message type to the appropriate destination.
        /// </summary>
        /// <param name="check">An action that performs assertions on the message.</param>
        public Saga<T> ExpectSendToDestination<TMessage>(Action<TMessage, string> check)
        {
            throw new NotImplementedException();

        }

        /// <summary>
        /// Check that the saga doesn't forward a message to the given destination.
        /// </summary>
        public Saga<T> ExpectNotForwardCurrentMessageTo(Func<string, bool> check)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectedNotForwardCurrentMessageTo(check));
            return this;
        }

        /// <summary>
        /// Check that the saga forwards a message to the given destination.
        /// </summary>
        public Saga<T> ExpectForwardCurrentMessageTo(Func<string, bool> check)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectedForwardCurrentMessageTo(check));
            return this;
        }

        /// <summary>
        /// Check that the saga replies to the originator with the given message type.
        /// </summary>
        public Saga<T> ExpectReplyToOriginator<TMessage>(Func<TMessage, bool> check = null)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectedReplyToOriginator<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the saga replies to the originator with the given message type.
        /// </summary>
        /// <param name="check">An action that performs assertions on the message.</param>
        public Saga<T> ExpectReplyToOriginator<TMessage>(Action<TMessage> check)
        {
            return ExpectReplyToOriginator(CheckActionToFunc(check));
        }

        /// <summary>
        /// Check that the saga publishes a message of the given type complying with the given predicate.
        /// </summary>
        public Saga<T> ExpectPublish<TMessage>(Func<TMessage, bool> check = null)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectedInvocations.ExpectedPublishInvocation<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the saga publishes a message of the given type complying with the given predicate.
        /// </summary>
        /// <param name="check">An action that performs assertions on the message.</param>
        public Saga<T> ExpectPublish<TMessage>(Action<TMessage> check)
        {
            return ExpectPublish(CheckActionToFunc(check));
        }

        /// <summary>
        /// Check that the saga does not publish any messages of the given type complying with the given predicate.
        /// </summary>
        public Saga<T> ExpectNotPublish<TMessage>(Func<TMessage, bool> check = null)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectedInvocations.ExpectedNotPublishInvocation<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the saga does not publish any messages of the given type complying with the given predicate.
        /// </summary>
        /// <param name="check">An action that performs assertions on the message.</param>
        public Saga<T> ExpectNotPublish<TMessage>(Action<TMessage> check)
        {
            return ExpectNotPublish(CheckActionToFunc(check));
        }

        /// <summary>
        /// Check that the saga tells the bus to handle the current message later.
        /// </summary>
        public Saga<T> ExpectHandleCurrentMessageLater()
        {
            throw new NotImplementedException();

        }

        /// <summary>
        /// Initializes the given message type and checks all the expectations previously set up,
        /// and then clears them for continued testing.
        /// </summary>
        public Saga<T> WhenHandling<TMessage>(Action<TMessage> initializeMessage = null)
        {
            var message = messageCreator.CreateInstance(initializeMessage);

            return When((s, c) => ((dynamic)s).Handle(message, c));
        }

        /// <summary>
        /// Initializes the given timeout message type and checks all the expectations previously set up,
        /// and then clears them for continued testing.
        /// </summary>
        public Saga<T> WhenHandlingTimeout<TMessage>(Action<TMessage> initializeMessage = null)
        {
            var message = messageCreator.CreateInstance(initializeMessage);

            return When((s, c) => ((dynamic)s).Timeout(message, c));
        }

        /// <summary>
        /// Uses the given delegate to invoke the saga, checking all the expectations previously set up,
        /// and then clearing them for continued testing.
        /// </summary>
        [ObsoleteEx(
            RemoveInVersion = "7",
            TreatAsErrorFromVersion = "6",
            Replacement = "When(Action<T, IMessageHandlerContext> sagaIsInvoked)")]
        public Saga<T> When(Action<T> sagaIsInvoked)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Uses the given delegate to invoke the saga, checking all the expectations previously set up,
        /// and then clearing them for continued testing.
        /// </summary>
        public Saga<T> When(Func<T, IMessageHandlerContext, Task> sagaIsInvoked)
        {
            // var id = messageId();

            sagaIsInvoked(saga, testableMessageHandlerContext).GetAwaiter().GetResult();

            testableMessageHandlerContext.Validate();
            testableMessageHandlerContext.Clear();

            // messageId = () => Guid.NewGuid().ToString();
            return this;
        }

        /// <summary>
        /// Invokes the saga timeout passing in the last timeout state it sent
        /// and then clears out all previous expectations.
        /// </summary>
        [ObsoleteEx(
            RemoveInVersion = "7",
            TreatAsErrorFromVersion = "6",
            Replacement = "WhenHandlingTimeOut(Action<TMessage> initializeMessage = null)")]
        public Saga<T> WhenSagaTimesOut()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Asserts that the saga is either complete or not.
        /// </summary>
        public Saga<T> AssertSagaCompletionIs(bool complete)
        {
            throw new NotImplementedException();    
        }

        /// <summary>
        /// Verifies that the saga is setting the specified timeout
        /// </summary>
        public Saga<T> ExpectTimeoutToBeSetIn<TMessage>(Func<TMessage, TimeSpan, bool> check = null)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectedDelayDeliveryWithInvocation<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Verifies that the saga is setting the specified timeout
        /// </summary>
        public Saga<T> ExpectTimeoutToBeSetIn<TMessage>(Action<TMessage, TimeSpan> check)
        {
            return ExpectTimeoutToBeSetIn(CheckActionToFunc(check));
        }

        /// <summary>
        /// Verifies that the saga is not setting the specified timeout
        /// </summary>
        public Saga<T> ExpectNoTimeoutToBeSetIn<TMessage>(Func<TMessage, TimeSpan, bool> check = null)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectedDelayDeliveryWithInvocation<TMessage>(check, true));
            return this;
        }

        /// <summary>
        /// Verifies that the saga is setting the specified timeout
        /// </summary>
        public Saga<T> ExpectTimeoutToBeSetAt<TMessage>(Func<TMessage, DateTime, bool> check = null)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectedDoNotDeliverBeforeInvocation<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Verifies that the saga is setting the specified timeout
        /// </summary>
        public Saga<T> ExpectTimeoutToBeSetAt<TMessage>(Action<TMessage, DateTime> check)
        {
            return ExpectTimeoutToBeSetAt(CheckActionToFunc(check));
        }

        /// <summary>
        /// Verifies that the saga is not setting the specified timeout
        /// </summary>
        public Saga<T> ExpectNoTimeoutToBeSetAt<TMessage>(Func<TMessage, DateTime, bool> check = null)
        {
            testableMessageHandlerContext.ExpectedInvocations.Add(new ExpectedDoNotDeliverBeforeInvocation<TMessage>(check, true));
            return this;
        }

        private static Func<T1, bool> CheckActionToFunc<T1>(Action<T1> check)
        {
            return arg =>
            {
                check(arg);
                return true;
            };
        }

        static Func<T1, T2, bool> CheckActionToFunc<T1, T2>(Action<T1, T2> check)
        {
            return (arg1, arg2) =>
            {
                check(arg1, arg2);
                return true;
            };
        }
    }
}