namespace NServiceBus.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using NServiceBus.Extensibility;
    using NServiceBus.Persistence;
    using NServiceBus.Sagas;

    /// <summary>
    /// For testing a saga with multiple messages and changes in time.
    /// </summary>
    /// <typeparam name="TSaga">The type of saga being tested.</typeparam>
    /// <typeparam name="TSagaData">The type of data stored by the saga.</typeparam>
    [DebuggerDisplay("TestableSaga for {typeof(TSaga).FullName}")]
    public class TestableSaga<TSaga, TSagaData>
        where TSaga : NServiceBus.Saga<TSagaData>
        where TSagaData : class, IContainSagaData, new()
    {
        readonly Func<TSaga> sagaFactory;
        readonly Queue<QueuedSagaMessage> queue;
        readonly ISagaPersister persister;
        readonly SagaMapper sagaMapper;
        readonly List<Tuple<DateTime, OutgoingMessage<object, SendOptions>>> storedTimeouts;
        readonly Dictionary<Type, List<Tuple<Type, Func<object, object>>>> replySimulators;

        /// <summary>
        /// Create a tester for a saga.
        /// </summary>
        /// <param name="sagaFactory">
        /// A factory to create saga instances. Not necessary if the saga has a parameterless constructor.
        /// Use this to satisfy constructor dependencies for the test instead of using dependency injection.
        /// This delegate is called multiple times.
        /// </param>
        /// <param name="initialCurrentTime">
        /// Sets the initial value of <see cref="CurrentTime"/>.
        /// If not supplied, the default is <see cref="DateTime.UtcNow"/>.
        /// </param>
        public TestableSaga(Func<TSaga> sagaFactory = null, DateTime? initialCurrentTime = null)
        {
            this.sagaFactory = sagaFactory ?? Activator.CreateInstance<TSaga>;
            CurrentTime = initialCurrentTime ?? DateTime.UtcNow;

            queue = new Queue<QueuedSagaMessage>();
            persister = new NonDurableSagaPersister();
            storedTimeouts = new List<Tuple<DateTime, OutgoingMessage<object, SendOptions>>>();
            replySimulators = new Dictionary<Type, List<Tuple<Type, Func<object, object>>>>();

            sagaMapper = SagaMapper.Get<TSaga, TSagaData>(this.sagaFactory);
        }

        /// <summary>
        /// Shows the "current time" of the saga.
        /// The initial time defaults to <see cref="DateTime.UtcNow"/>.
        /// This can be changed by passing an initial current time in the <see cref="TestableSaga{TSaga,TSagaData}"/> constructor.
        /// The current time may be advanced by calling <see cref="AdvanceTime"/>,
        /// which also handle any timeouts due during the specified <see cref="TimeSpan"/>.
        /// </summary>
        public DateTime CurrentTime { get; private set; }

        /// <summary>
        /// <c>true</c> if there are any queued messages.
        /// </summary>
        public bool HasQueuedMessages => queue.Any();

        /// <summary>
        /// The length of the message queue.
        /// </summary>
        public int QueueLength => queue.Count();

        /// <summary>
        /// Peeks at the first message in the queue.
        /// </summary>
        /// <returns>Details about the next message in the message queue.</returns>
        /// <exception cref="InvalidOperationException">There are no queued messages.</exception>
        public QueuedSagaMessage QueuePeek() => queue.Peek();

        /// <summary>
        /// Handle a message in the saga.
        /// This uses <see cref="NServiceBus.Saga{TSagaData}.ConfigureHowToFindSaga(SagaPropertyMapper{TSagaData})"/>
        /// to find the correct saga data based on correlation ID values from the message,
        /// and invokes the correct handler method based on the message type.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="message">The message for the saga to handle.</param>
        /// <param name="context">
        /// An optional <see cref="TestableMessageHandlerContext"/> to use while handling the message.
        /// If none is supplied, an instance is created and returned as <see cref="HandleResult.Context"/>.
        /// </param>
        /// <param name="messageHeaders">
        /// Optional message headers to include with the message. Usually not needed unless
        /// <see cref="NServiceBus.Saga{TSagaData}.ConfigureHowToFindSaga(SagaPropertyMapper{TSagaData})"/>
        /// uses header mappings.
        /// </param>
        /// <returns>
        /// Returns a <see cref="HandleResult"/> containing a snapshot of the saga data after the message is handled
        /// and the <see cref="TestableMessageHandlerContext"/> containing details about context operations that occurred in the message
        /// handler, which can be used for test assertions.
        /// </returns>
        public Task<HandleResult> Handle<TMessage>(TMessage message, TestableMessageHandlerContext context = null, IReadOnlyDictionary<string, string> messageHeaders = null)
        {
            if (context == null)
            {
                context = new TestableMessageHandlerContext();
            }

            var queueMessage = new QueuedSagaMessage(typeof(TMessage), message, messageHeaders);
            return InnerHandle(queueMessage, "Handle", context);
        }

        /// <summary>
        /// Handle a reply in the saga, such as a reply from a handler the saga sent a command to.
        /// For replies, this method should be used instead of <see cref="Handle{TMessage}"/>
        /// to ensure the message is auto-correlated to the saga.
        /// </summary>
        /// <typeparam name="TMessage">The type of the message.</typeparam>
        /// <param name="sagaId">The saga Id that would be embedded in the message headers, because it was sent by the saga,
        /// or is an auto-correlated reply to a message sent by the saga.</param>
        /// <param name="message">The message for the saga to handle.</param>
        /// <param name="context">
        /// An optional <see cref="TestableMessageHandlerContext"/> to use while handling this message.
        /// If none is supplied, an instance is created and returned as <see cref="HandleResult.Context"/>.
        /// </param>
        /// <param name="messageHeaders">
        /// Optional message headers to include with the message. Usually not needed unless the
        /// <see cref="NServiceBus.Saga{TSagaData}.ConfigureHowToFindSaga(SagaPropertyMapper{TSagaData})"/> method uses header mappings.
        /// </param>
        /// <returns>
        /// Returns a <see cref="HandleResult"/> containing a snapshot of the saga data after the message is handled
        /// and the <see cref="TestableMessageHandlerContext"/> containing details about context operations that occurred in the message
        /// handler, which can be used for test assertions.
        /// </returns>
        public Task<HandleResult> HandleReply<TMessage>(Guid sagaId, TMessage message, TestableMessageHandlerContext context = null, IReadOnlyDictionary<string, string> messageHeaders = null)
        {
            var newHeaders = new Dictionary<string, string>();
            if (messageHeaders != null)
            {
                foreach (var pair in messageHeaders)
                {
                    newHeaders.Add(pair.Key, pair.Value);
                }
            }

            newHeaders[Headers.SagaId] = sagaId.ToString();

            return Handle(message, context, newHeaders);
        }

        /// <summary>
        /// Handle a queued message in the saga.
        /// Messages may be queued because the saga sends or publishes a message that is also handled by the saga
        /// or because a reply has been simulated using <see cref="SimulateReply{TMessageFromSaga,TResponseMessage}"/>.
        /// </summary>
        /// <param name="context">
        /// An optional <see cref="TestableMessageHandlerContext"/> to use while handling this message.
        /// If none is supplied, an instance is created and returned as <see cref="HandleResult.Context"/>.
        /// </param>
        /// <returns>
        /// Returns a <see cref="HandleResult"/> containing a snapshot of the saga data after the message is handled and
        /// the <see cref="TestableMessageHandlerContext"/> containing details about context operations that
        /// occurred in the message handler, which can be used for test assertions.
        /// </returns>
        public Task<HandleResult> HandleQueuedMessage(TestableMessageHandlerContext context = null)
        {
            if (!queue.Any())
            {
                throw new Exception("There are no queued messages.");
            }

            if (context == null)
            {
                context = new TestableMessageHandlerContext();
            }

            var queuedMessage = queue.Dequeue();
            return InnerHandle(queuedMessage, "Handle", context);
        }

        /// <summary>
        /// Simulates an external handler replying to a message sent from this saga when used in the commander style.
        /// The delegate is used to create a response message with headers that indicate it belongs to the correct saga data instance,
        /// which mimics how message handlers support auto-correlation when replying to messages from a saga.
        /// </summary>
        /// <typeparam name="TSagaMessage">The message type sent or published by the saga.</typeparam>
        /// <typeparam name="TReplyMessage">The message type that the external handler would reply with.</typeparam>
        /// <param name="simulateReply">
        /// Simulates a reply to a message sent by the saga. Returns <c>null</c> to simulate no reply.
        /// </param>
        public void SimulateReply<TSagaMessage, TReplyMessage>(Func<TSagaMessage, TReplyMessage> simulateReply)
        {
            if (!sagaMapper.HandlesMessageType(typeof(TReplyMessage)))
            {
                throw new Exception($"Messages of type {typeof(TReplyMessage).FullName} are not handled by the saga.");
            }

            if (!replySimulators.TryGetValue(typeof(TSagaMessage), out List<Tuple<Type, Func<object, object>>> simulators))
            {
                simulators = new List<Tuple<Type, Func<object, object>>>();
                replySimulators.Add(typeof(TSagaMessage), simulators);
            }

            simulators.Add(new Tuple<Type, Func<object, object>>(typeof(TReplyMessage), message => simulateReply((TSagaMessage)message)));
        }

        async Task<HandleResult> InnerHandle(QueuedSagaMessage message, string handleMethodName, TestableMessageHandlerContext context)
        {
            var saga = sagaFactory();

            using (var session = new NonDurableSynchronizedStorageSession())
            {
                var contextBag = new ContextBag();

                var loadResult = await LoadSagaData(message, session, contextBag).ConfigureAwait(false);
                saga.Entity = loadResult.Item1;

                await sagaMapper.InvokeHandlerMethod(saga, handleMethodName, message, context).ConfigureAwait(false);
                await SaveSagaData(saga, loadResult.Item2, loadResult.Item3, session, contextBag).ConfigureAwait(false);
                await session.CompleteAsync().ConfigureAwait(false);
            }

            EnqueueMessagesAndTimeouts(context, saga.Entity.Id);

            return new HandleResult(saga, message, context);
        }


        /// <summary>
        /// Advance the <see cref="CurrentTime"/> for the saga and handle any timeouts due during <paramref name="timeToAdvance"/>.
        /// </summary>
        /// <param name="timeToAdvance">Amount of time to advance the <see cref="CurrentTime"/>.</param>
        /// <param name="provideContext">A factory to provide a custom <see cref="TestableMessageHandlerContext"/> for each handled timeout if needed.</param>
        /// <returns>
        /// Returns an array of <see cref="HandleResult"/>,
        /// one for each timeout handled during <paramref name="timeToAdvance"/>.
        /// If no timeouts were handled, the array is empty.
        /// </returns>
        public async Task<HandleResult[]> AdvanceTime(TimeSpan timeToAdvance, Func<OutgoingMessage<object, SendOptions>, TestableMessageHandlerContext> provideContext = null)
        {
            var contextFactory = provideContext ?? new Func<OutgoingMessage<object, SendOptions>, TestableMessageHandlerContext>(timeout => new TestableMessageHandlerContext());
            var advanceToTime = CurrentTime + timeToAdvance;

            var due = storedTimeouts.Where(t => t.Item1 <= advanceToTime).OrderBy(t => t.Item2).ToArray();
            storedTimeouts.RemoveAll(t => t.Item1 <= advanceToTime);

            var results = new List<HandleResult>();

            foreach (var storedTimeout in due)
            {
                CurrentTime = storedTimeout.Item1;
                var context = contextFactory(storedTimeout.Item2);
                var result = await HandleTimeout(storedTimeout.Item2, context).ConfigureAwait(false);
                results.Add(result);
            }

            CurrentTime = advanceToTime;

            return results.ToArray();
        }

        async Task<HandleResult> HandleTimeout(OutgoingMessage<object, SendOptions> timeoutMessage, TestableMessageHandlerContext context)
        {
            var messageType = timeoutMessage.Message.GetType();

            var queueMessage = new QueuedSagaMessage(messageType, timeoutMessage.Message, timeoutMessage.Options.GetHeaders());

            var methodName = timeoutMessage is TimeoutMessage<object> ? "Timeout" : "Handle";

            return await InnerHandle(queueMessage, methodName, context).ConfigureAwait(false);
        }

        async Task<Tuple<TSagaData, bool, object>> LoadSagaData(QueuedSagaMessage message, SynchronizedStorageSession session, ContextBag contextBag)
        {
            var messageMetadata = sagaMapper.GetMessageMetadata(message.Type);
            TSagaData sagaData;

            if (message.Headers != null &&
                message.Headers.TryGetValue(Headers.SagaId, out var sagaIdString) &&
                Guid.TryParse(sagaIdString, out Guid sagaId))
            {
                sagaData = await persister.Get<TSagaData>(sagaId, session, contextBag).ConfigureAwait(false);
                if (sagaData != null)
                {
                    return new Tuple<TSagaData, bool, object>(sagaData, false, null);
                }
            }

            var messageMappedValue = sagaMapper.GetMessageMappedValue(message);

            sagaData = await persister.Get<TSagaData>(sagaMapper.CorrelationPropertyName, messageMappedValue, session, contextBag).ConfigureAwait(false);

            if (sagaData != null)
            {
                return new Tuple<TSagaData, bool, object>(sagaData, false, messageMappedValue);
            }

            if (messageMetadata.IsAllowedToStartSaga)
            {
                sagaData = new TSagaData { Id = Guid.NewGuid() };
                sagaMapper.SetCorrelationPropertyValue(sagaData, messageMappedValue);
                return new Tuple<TSagaData, bool, object>(sagaData, true, messageMappedValue);
            }

            throw new Exception($"Saga not found and message type {message.Type.FullName} is not allowed to start the saga.");
        }

        async Task SaveSagaData(TSaga saga, bool isNew, object mappedValue, SynchronizedStorageSession session, ContextBag contextBag)
        {
            if (saga.Completed)
            {
                await persister.Complete(saga.Entity, session, contextBag).ConfigureAwait(false);
            }
            else if (isNew)
            {
                var saveCorrelationProperty = new SagaCorrelationProperty(sagaMapper.CorrelationPropertyName, mappedValue);
                await persister.Save(saga.Entity, saveCorrelationProperty, session, contextBag).ConfigureAwait(false);
            }
            else
            {
                await persister.Update(saga.Entity, session, contextBag).ConfigureAwait(false);
            }
        }

        void EnqueueMessagesAndTimeouts(TestableMessageHandlerContext context, Guid sagaId)
        {
            foreach (var timeout in context.TimeoutMessages)
            {
                if (timeout.Within.HasValue)
                {
                    var targetTime = CurrentTime + timeout.Within.Value;
                    storedTimeouts.Add(new Tuple<DateTime, OutgoingMessage<object, SendOptions>>(targetTime, timeout));
                }
                else if (timeout.At.HasValue)
                {
                    storedTimeouts.Add(new Tuple<DateTime, OutgoingMessage<object, SendOptions>>(timeout.At.Value.UtcDateTime, timeout));
                }
                else
                {
                    throw new Exception("The timeout message has no delivery time. (This really shouldn't happen.)");
                }
            }

            foreach (var message in context.SentMessages)
            {
                var headers = message.Options.GetHeaders();
                if (headers.TryGetValue(Headers.IsSagaTimeoutMessage, out _))
                {
                    // Already dealt with saga timeouts above
                    continue;
                }

                var msgType = message.Message.GetType();
                if (sagaMapper.HandlesMessageType(msgType))
                {
                    var deliveryDelay = message.Options.GetDeliveryDelay();
                    var deliverAt = message.Options.GetDeliveryDate();
                    if (deliveryDelay.HasValue)
                    {
                        var targetTime = CurrentTime + deliveryDelay.Value;
                        storedTimeouts.Add(new Tuple<DateTime, OutgoingMessage<object, SendOptions>>(targetTime, message));
                    }
                    else if (deliverAt.HasValue)
                    {
                        storedTimeouts.Add(new Tuple<DateTime, OutgoingMessage<object, SendOptions>>(deliverAt.Value.UtcDateTime, message));
                    }
                    else
                    {
                        var queuedMessage = new QueuedSagaMessage(msgType, message.Message, headers, sagaId);
                        queue.Enqueue(queuedMessage);
                    }
                }
                else if (replySimulators.TryGetValue(msgType, out var simulators))
                {
                    foreach (var sim in simulators)
                    {
                        var replyMessageType = sim.Item1;
                        var simulateReply = sim.Item2;
                        var repliedMessage = simulateReply(message.Message);
                        if (repliedMessage != null)
                        {
                            var queuedMessage = new QueuedSagaMessage(replyMessageType, repliedMessage, autoCorrelatedSagaId: sagaId);
                            queue.Enqueue(queuedMessage);
                        }
                    }
                }
            }

            foreach (var message in context.PublishedMessages)
            {
                var msgType = message.Message.GetType();
                if (sagaMapper.HandlesMessageType(msgType))
                {
                    var queuedMessage = new QueuedSagaMessage(msgType, message.Message, message.Options.GetHeaders(), sagaId);
                    queue.Enqueue(queuedMessage);
                }
            }
        }

        /// <summary>
        /// The result of a saga handling a message.
        /// </summary>
        [DebuggerDisplay("HandleResult: {HandledMessage.Message}")]
        public class HandleResult
        {
            internal HandleResult(NServiceBus.Saga<TSagaData> saga, QueuedSagaMessage handledMessage, TestableMessageHandlerContext context)
            {
                SagaId = saga.Data.Id;
                Completed = saga.Completed;
                HandledMessage = handledMessage;
                Context = context;

                SagaDataSnapshot = saga.Data.DeepCopy();
            }

            /// <summary>
            /// The Id of the saga that was created or retrieved from storage.
            /// </summary>
            public Guid SagaId { get; }

            /// <summary>
            /// True if the result of the message handler was that the <see cref="Saga.MarkAsComplete"/> method was called.
            /// </summary>
            public bool Completed { get; }

            /// <summary>
            /// Details about the message that was just handled.
            /// </summary>
            public QueuedSagaMessage HandledMessage { get; }

            /// <summary>
            /// The <see cref="IMessageHandlerContext"/> that was used to handle the message.
            /// The context contains details of what happened during the handler method and can be used for test assertions.
            /// </summary>
            public TestableMessageHandlerContext Context { get; }

            /// <summary>
            /// A copy of the saga data after the message was handled.
            /// </summary>
            public TSagaData SagaDataSnapshot { get; }

            /// <summary>
            /// Returns the first published message of a given type,
            /// or a default value if there is no published message of the given type.
            /// </summary>
            public TMessage FindPublishedMessage<TMessage>() => Context.FindPublishedMessage<TMessage>();

            /// <summary>
            /// Returns the first sent message of a given type,
            /// or a default value if there is no sent message of the given type.
            /// </summary>
            public TMessage FindSentMessage<TMessage>() => Context.FindSentMessage<TMessage>();

            /// <summary>
            /// Returns the first timeout message of a given type,
            /// or a default value if there is no timeout message of the given type.
            /// </summary>
            public TMessage FindTimeoutMessage<TMessage>() => Context.FindTimeoutMessage<TMessage>();

            /// <summary>
            /// Returns the first replied message of a given type,
            /// or a default value if there is no replied message of the given type.
            /// </summary>
            public TMessage FindReplyMessage<TMessage>() => Context.FindReplyMessage<TMessage>();
        }
    }
}