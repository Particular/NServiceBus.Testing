namespace NServiceBus.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using MessageInterfaces.MessageMapper.Reflection;

    internal static class SagaConsts
    {
        public const string Originator = "NServiceBus.Testing.SagaOriginator";
    }

    /// <summary>
    /// Saga unit testing framework.
    /// </summary>
    public class Saga<T> where T : Saga
    {
        internal Saga(T saga)
        {
            this.saga = saga;
            testContext = new TestingContext(messageCreator);

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
            saga.Entity.Originator = SagaConsts.Originator;
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
        /// Provides a way to set external dependencies on the saga under test.
        /// </summary>
        public Saga<T> WithExternalDependencies(Action<T> actionToSetUpExternalDependencies)
        {
            actionToSetUpExternalDependencies(saga);
            return this;
        }

        /// <summary>
        /// Provides a way to customize the <see cref="IMessageHandlerContext" /> instance received by the message handler.
        /// </summary>
        public Saga<T> ConfigureHandlerContext(Action<TestableMessageHandlerContext> contextInitializer)
        {
            contextInitializer(testContext);
            return this;
        }

        /// <summary>
        /// Set the headers on an incoming message that will be return
        /// when code calls Bus.CurrentMessageContext.Headers
        /// </summary>
        public Saga<T> SetIncomingHeader(string key, string value)
        {
            testContext.MessageHeaders[key] = value;
            return this;
        }

        /// <summary>
        /// Check that the saga sends a message of the given type complying with the given predicate.
        /// </summary>
        public Saga<T> ExpectSend<TMessage>(Func<TMessage, SendOptions, bool> check = null)
        {
            testContext.AddExpectation(new ExpectSend<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the saga sends a message of the given type complying with the given predicate.
        /// </summary>
        public Saga<T> ExpectSend<TMessage>(Func<TMessage, bool> check)
        {
            return ExpectSend((TMessage m, SendOptions _) => check(m));
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
        public Saga<T> ExpectNotSend<TMessage>(Func<TMessage, SendOptions, bool> check = null)
        {
            testContext.AddExpectation(new ExpectNotSend<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the saga does not send a message of the given type complying with the given predicate.
        /// </summary>
        public Saga<T> ExpectNotSend<TMessage>(Func<TMessage, bool> check)
        {
            return ExpectNotSend((TMessage m, SendOptions _) => check(m));
        }

        /// <summary>
        /// Check that the saga does not send a message of the given type complying with the given predicate.
        /// </summary>
        /// <param name="check">An action containing assertions on the message.</param>
        public Saga<T> ExpectNotSend<TMessage>(Action<TMessage> check)
        {
            return ExpectNotSend(CheckActionToFunc(check));
        }

        /// <summary>
        /// Check that the saga replies with the given message type complying with the given predicate.
        /// </summary>
        public Saga<T> ExpectReply<TMessage>(Func<TMessage, ReplyOptions, bool> check = null)
        {
            testContext.AddExpectation(new ExpectReply<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the saga replies with the given message type complying with the given predicate.
        /// </summary>
        public Saga<T> ExpectReply<TMessage>(Func<TMessage, bool> check)
        {
            return ExpectReply((TMessage m, ReplyOptions _) => check(m));
        }

        /// <summary>
        /// Check that the saga does not reply with the given message type complying with the given predicate.
        /// </summary>
        public Saga<T> ExpectNotReply<TMessage>(Func<TMessage, ReplyOptions, bool> check = null)
        {
            testContext.AddExpectation(new ExpectNotReply<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the saga does not reply with the given message type complying with the given predicate.
        /// </summary>
        public Saga<T> ExpectNotReply<TMessage>(Func<TMessage, bool> check)
        {
            return ExpectNotReply((TMessage m, ReplyOptions _) => check(m));
        }

        /// <summary>
        /// Check that the saga doesn't forward a message to the given destination.
        /// </summary>
        public Saga<T> ExpectNotForwardCurrentMessageTo(Func<string, bool> check = null)
        {
            testContext.AddExpectation(new ExpectNotForwardCurrentMessageTo(check));
            return this;
        }

        /// <summary>
        /// Check that the saga forwards a message to the given destination.
        /// </summary>
        public Saga<T> ExpectForwardCurrentMessageTo(Func<string, bool> check = null)
        {
            testContext.AddExpectation(new ExpectForwardCurrentMessageTo(check));
            return this;
        }

        /// <summary>
        /// Check that the saga replies to the originator with the given message type.
        /// </summary>
        public Saga<T> ExpectReplyToOriginator<TMessage>(Func<TMessage, bool> check = null)
        {
            testContext.AddExpectation(new ExpectReplyToOriginator<TMessage>(check));
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
        public Saga<T> ExpectPublish<TMessage>(Func<TMessage, PublishOptions, bool> check = null)
        {
            testContext.AddExpectation(new ExpectPublish<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the saga publishes a message of the given type complying with the given predicate.
        /// </summary>
        public Saga<T> ExpectPublish<TMessage>(Func<TMessage, bool> check)
        {
            return ExpectPublish((TMessage m, PublishOptions _) => check(m));
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
        public Saga<T> ExpectNotPublish<TMessage>(Func<TMessage, PublishOptions, bool> check = null)
        {
            testContext.AddExpectation(new ExpectNotPublish<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the saga does not publish any messages of the given type complying with the given predicate.
        /// </summary>
        public Saga<T> ExpectNotPublish<TMessage>(Func<TMessage, bool> check)
        {
            return ExpectNotPublish((TMessage m, PublishOptions _) => check(m));
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
        /// Initializes the given message type and checks all the expectations previously set up,
        /// and then clears them for continued testing.
        /// </summary>
        public Saga<T> WhenHandling<TMessage>(Action<TMessage> initializeMessage = null)
        {
            return WhenHandlingAsync(initializeMessage).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Checks all the expectations previously set up, and then clears them for continued testing.
        /// </summary>
        public Saga<T> WhenHandling<TMessage>(TMessage message)
        {
            return WhenHandlingAsync(message).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Initializes the given timeout message type and checks all the expectations previously set up,
        /// and then clears them for continued testing.
        /// </summary>
        public Saga<T> WhenHandlingTimeout<TMessage>(Action<TMessage> initializeMessage = null)
        {
            return WhenHandlingTimeoutAsync(initializeMessage).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Checks all the expectations previously set up, and then clears them for continued testing.
        /// </summary>
        public Saga<T> WhenHandlingTimeout<TMessage>(TMessage message)
        {
            return WhenHandlingTimeoutAsync(message).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Uses the given delegate to invoke the saga, checking all the expectations previously set up,
        /// and then clearing them for continued testing.
        /// example: <c>When((saga, context) => s.Handle(new MyMessage(), context))</c>
        /// </summary>
        public Saga<T> When(Func<T, IMessageHandlerContext, Task> sagaIsInvoked)
        {
            return WhenAsync(sagaIsInvoked).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Uses the given delegate to select the message handler and invoking it with the given message. Checks all the expectations previously, and then clearing them for continued testing.
        /// example: <c>When(s => s.Handle, new MyMessage())</c>
        /// </summary>
        public Saga<T> When<TMessage>(Func<T, Func<TMessage, IMessageHandlerContext, Task>> handlerSelector, TMessage message)
        {
            return WhenAsync((s, context) => handlerSelector(s)(message, context)).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Uses the given delegate to select the message handler and invoking it with the specified message. Checks all the expectations previously, and then clearing them for continued testing.
        /// example: <c>When&lt;MyMessage>(s => s.Handle, m => { m.Value = 42 })</c>
        /// </summary>
        public Saga<T> When<TMessage>(Func<T, Func<TMessage, IMessageHandlerContext, Task>> handlerSelector, Action<TMessage> messageInitializer = null)
        {
            return WhenAsync(handlerSelector, messageInitializer).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Expires requested timeouts for the saga by simulating that time has passed
        /// and then clears out all previous expectations.
        /// This will only invoke timeouts set with a <see cref="TimeSpan"/> argument.
        /// </summary>
        /// <param name="after">The amount of time that has passed to simulate.</param>
        public Saga<T> WhenSagaTimesOut(TimeSpan after)
        {
            return WhenSagaTimesOutAsync(after).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Expires requested timeouts for the saga by simulating the passed in date and time
        /// and then clears out all previous expectations.
        /// This will only invoke timeouts set with a <see cref="DateTime"/> argument.
        /// </summary>
        /// <param name="at">The Date and time to simuluate.</param>
        public Saga<T> WhenSagaTimesOut(DateTime at)
        {
            return WhenSagaTimesOutAsync(at).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Expires all requested timeouts for the saga and then clears out all previous expectations.
        /// </summary>
        public Saga<T> WhenSagaTimesOut()
        {
            return WhenSagaTimesOutAsync().GetAwaiter().GetResult();
        }

        /// <summary>
        /// Initializes the given message type and checks all the expectations previously set up,
        /// and then clears them for continued testing.
        /// </summary>
        public Task<Saga<T>> WhenHandlingAsync<TMessage>(Action<TMessage> initializeMessage = null)
        {
            var message = messageCreator.CreateInstance(initializeMessage);
            return WhenHandlingAsync(message);
        }

        /// <summary>
        /// Checks all the expectations previously set up, and then clears them for continued testing.
        /// </summary>
        public Task<Saga<T>> WhenHandlingAsync<TMessage>(TMessage message)
        {
            var invokers = saga.GetType().CreateInvokers(typeof(TMessage), typeof(IHandleMessages<>));
            return WhenAsync((s, c) => invokers.InvokeSerially(saga, message, c));
        }

        /// <summary>
        /// Initializes the given timeout message type and checks all the expectations previously set up,
        /// and then clears them for continued testing.
        /// </summary>
        public Task<Saga<T>> WhenHandlingTimeoutAsync<TMessage>(Action<TMessage> initializeMessage = null)
        {
            var message = messageCreator.CreateInstance(initializeMessage);
            return WhenHandlingTimeoutAsync(message);
        }

        /// <summary>
        /// Checks all the expectations previously set up, and then clears them for continued testing.
        /// </summary>
        public Task<Saga<T>> WhenHandlingTimeoutAsync<TMessage>(TMessage message)
        {
            var invokers = saga.GetType().CreateInvokers(typeof(TMessage), typeof(IHandleTimeouts<>));
            return WhenAsync((s, c) => invokers.InvokeSerially(saga, message, c));
        }

        /// <summary>
        /// Uses the given delegate to invoke the saga, checking all the expectations previously set up,
        /// and then clearing them for continued testing.
        /// example: <c>When((saga, context) => s.Handle(new MyMessage(), context))</c>
        /// </summary>
        public async Task<Saga<T>> WhenAsync(Func<T, IMessageHandlerContext, Task> sagaIsInvoked)
        {
            await sagaIsInvoked(saga, testContext).ConfigureAwait(false);

            testContext.Validate();
            testContext = new TestingContext(messageCreator, testContext.previousTimeouts.Concat(testContext.TimeoutMessages).ToArray());

            return this;
        }

        /// <summary>
        /// Uses the given delegate to select the message handler and invoking it with the given message. Checks all the expectations previously, and then clearing them for continued testing.
        /// example: <c>When(s => s.Handle, new MyMessage())</c>
        /// </summary>
        public Task<Saga<T>> WhenAsync<TMessage>(Func<T, Func<TMessage, IMessageHandlerContext, Task>> handlerSelector, TMessage message)
        {
            return WhenAsync((s, context) => handlerSelector(s)(message, context));
        }

        /// <summary>
        /// Uses the given delegate to select the message handler and invoking it with the specified message. Checks all the expectations previously, and then clearing them for continued testing.
        /// example: <c>When&lt;MyMessage>(s => s.Handle, m => { m.Value = 42 })</c>
        /// </summary>
        public Task<Saga<T>> WhenAsync<TMessage>(Func<T, Func<TMessage, IMessageHandlerContext, Task>> handlerSelector, Action<TMessage> messageInitializer = null)
        {
            var message = (TMessage)messageCreator.CreateInstance(typeof(TMessage));
            messageInitializer?.Invoke(message);
            return WhenAsync((s, context) => handlerSelector(s)(message, context));
        }

        /// <summary>
        /// Expires requested timeouts for the saga by simulating that time has passed
        /// and then clears out all previous expectations.
        /// This will only invoke timeouts set with a <see cref="TimeSpan"/> argument.
        /// </summary>
        /// <param name="after">The amount of time that has passed to simulate.</param>
        public async Task<Saga<T>> WhenSagaTimesOutAsync(TimeSpan after)
        {
            await InvokeTimeoutsAsync(testContext.previousTimeouts
                .Where(t => t.Within.HasValue)
                .Where(t => t.Within <= after)).ConfigureAwait(false);

            return this;
        }

        /// <summary>
        /// Expires requested timeouts for the saga by simulating the passed in date and time
        /// and then clears out all previous expectations.
        /// This will only invoke timeouts set with a <see cref="DateTime"/> argument.
        /// </summary>
        /// <param name="at">The Date and time to simuluate.</param>
        public async Task<Saga<T>> WhenSagaTimesOutAsync(DateTime at)
        {
            await InvokeTimeoutsAsync(testContext.previousTimeouts
                .Where(t => t.At.HasValue)
                .Where(t => t.At <= at)).ConfigureAwait(false);

            return this;
        }

        /// <summary>
        /// Expires all requested timeouts for the saga and then clears out all previous expectations.
        /// </summary>
        public async Task<Saga<T>> WhenSagaTimesOutAsync()
        {
            await InvokeTimeoutsAsync(testContext.previousTimeouts).ConfigureAwait(false);

            return this;
        }

        /// <summary>
        /// Verifies that the saga has been completed.
        /// </summary>
        public Saga<T> ExpectSagaCompleted()
        {
            testContext.AddExpectation(new ExpectSagaCompleted<T>(saga, true));
            return this;
        }

        /// <summary>
        /// Verifies that the saga has not been completed.
        /// </summary>
        public Saga<T> ExpectSagaNotCompleted()
        {
            testContext.AddExpectation(new ExpectSagaCompleted<T>(saga, false));
            return this;
        }

        /// <summary>
        /// Verifies that the saga is setting the specified timeout
        /// </summary>
        public Saga<T> ExpectTimeoutToBeSetIn<TMessage>(Func<TMessage, TimeSpan, bool> check = null)
        {
            testContext.AddExpectation(new ExpectDelayDeliveryWith<TMessage>(check));
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
            testContext.AddExpectation(new ExpectNotDelayDeliveryWith<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Verifies that the saga is setting the specified timeout
        /// </summary>
        public Saga<T> ExpectTimeoutToBeSetAt<TMessage>(Func<TMessage, DateTimeOffset, bool> check = null)
        {
            testContext.AddExpectation(new ExpectDoNotDeliverBefore<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Verifies that the saga is setting the specified timeout
        /// </summary>
        public Saga<T> ExpectTimeoutToBeSetAt<TMessage>(Action<TMessage, DateTimeOffset> check)
        {
            return ExpectTimeoutToBeSetAt(CheckActionToFunc(check));
        }

        /// <summary>
        /// Verifies that the saga is not setting the specified timeout
        /// </summary>
        public Saga<T> ExpectNoTimeoutToBeSetAt<TMessage>(Func<TMessage, DateTimeOffset, bool> check = null)
        {
            testContext.AddExpectation(new ExpectNotDoNotDeliverBefore<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the saga sends the given message type to its local queue
        /// and that the message complies with the given predicate.
        /// </summary>
        public Saga<T> ExpectSendLocal<TMessage>(Func<TMessage, bool> check = null)
        {
            testContext.AddExpectation(new ExpectSendLocal<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the saga sends the given message type to its local queue
        /// and that the message complies with the given predicate.
        /// </summary>
        /// <param name="check">An action that performs assertions on the message.</param>
        public Saga<T> ExpectSendLocal<TMessage>(Action<TMessage> check)
        {
            return ExpectSendLocal(CheckActionToFunc(check));
        }

        /// <summary>
        /// Check that the saga does not send a message type to its local queue that complies with the given predicate.
        /// </summary>
        public Saga<T> ExpectNotSendLocal<TMessage>(Func<TMessage, bool> check = null)
        {
            testContext.AddExpectation(new ExpectNotSendLocal<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the saga does not send a message type to its local queue that complies with the given predicate.
        /// </summary>
        /// <param name="check">An action that performs assertions on the message.</param>
        public Saga<T> ExpectNotSendLocal<TMessage>(Action<TMessage> check)
        {
            return ExpectNotSendLocal(CheckActionToFunc(check));
        }

        /// <summary>
        /// Check that the saga sends the given message type to the appropriate destination.
        /// </summary>
        public Saga<T> ExpectSendToDestination<TMessage>(Func<TMessage, string, bool> check = null)
        {
            testContext.AddExpectation(new ExpectSendToDestination<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the saga does not send the given message type to the given destination.
        /// </summary>
        /// <param name="check">An action that performs assertions on the message.</param>
        public Saga<T> ExpectNotSendToDestination<TMessage>(Action<TMessage, string> check)
        {
            return ExpectNotSendToDestination(CheckActionToFunc(check));
        }

        /// <summary>
        /// Check that the saga does not send the given message type to the given destination.
        /// </summary>
        public Saga<T> ExpectNotSendToDestination<TMessage>(Func<TMessage, string, bool> check = null)
        {
            testContext.AddExpectation(new ExpectNotSendToDestination<TMessage>(check));
            return this;
        }

        /// <summary>
        /// Check that the saga sends the given message type to the appropriate destination.
        /// </summary>
        /// <param name="check">An action that performs assertions on the message.</param>
        public Saga<T> ExpectSendToDestination<TMessage>(Action<TMessage, string> check)
        {
            return ExpectSendToDestination(CheckActionToFunc(check));
        }

        /// <summary>
        /// Check that the saga data matches the given type and comlies with the given predicate.
        /// </summary>
        public Saga<T> ExpectSagaData<TSagaData>(Func<TSagaData, bool> check) where TSagaData : IContainSagaData
        {
            testContext.AddExpectation(new ExpectSagaData<TSagaData>(saga, check));
            return this;
        }

        async Task InvokeTimeoutsAsync(IEnumerable<TimeoutMessage<object>> messages)
        {
            var timeoutHandlers = messages.OrderBy(t => t.Within).ToList();

            foreach (var timeoutHandler in timeoutHandlers)
            {
                var messageType = messageCreator.GetMappedTypeFor(timeoutHandler.Message.GetType());
                var invokers = saga.GetType().CreateInvokers(messageType, typeof(IHandleTimeouts<>));
                await invokers.InvokeSerially(saga, timeoutHandler.Message, testContext).ConfigureAwait(false);
            }

            testContext.Validate();
            testContext = new TestingContext(messageCreator);
        }

        static Func<T1, bool> CheckActionToFunc<T1>(Action<T1> check)
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

        readonly T saga;

        MessageMapper messageCreator = new MessageMapper();

        TestingContext testContext;
    }
}