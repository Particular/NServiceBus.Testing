namespace NServiceBus.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using NServiceBus.Extensibility;
    using NServiceBus.Persistence;
    using NServiceBus.Sagas;

    /// <summary>
    /// Enables testing of a saga through multiple messages.
    /// </summary>
    /// <typeparam name="TSaga">The saga type being tested.</typeparam>
    /// <typeparam name="TSagaEntity">The saga data class associated with the saga being tested.</typeparam>
    [DebuggerDisplay("TestableSaga for {typeof(TSaga).FullName}")]
    public class TestableSaga<TSaga, TSagaEntity>
        where TSaga : NServiceBus.Saga<TSagaEntity>
        where TSagaEntity : class, IContainSagaData, new()
    {
        readonly Func<TSaga> sagaFactory;
        readonly Queue<QueuedSagaMessage> queue;
        readonly ISagaPersister persister;
        readonly SagaMapper sagaMapper;
        readonly List<(DateTime At, OutgoingMessage<object, SendOptions> Timeout)> storedTimeouts;
        readonly Dictionary<Type, List<(Type, Func<object, object>)>> handlerSimulations;

        /// <summary>
        /// Create a tester for a saga.
        /// </summary>
        /// <param name="sagaFactory">
        /// A factory to create saga instances. Not necessary if the saga has a parameterless constructor.
        /// Use this to satisfy constructor dependencies for the test instead of using dependency injection.
        /// This delegate will be called multiple times.
        /// </param>
        /// <param name="initialDateTime">
        /// Supplies a <see cref="CurrentTime"/> for the.
        /// If not supplied, defaults to the current server time.</param>
        public TestableSaga(Func<TSaga> sagaFactory = null, DateTime? initialDateTime = null)
        {
            this.sagaFactory = sagaFactory ?? (() => Activator.CreateInstance<TSaga>());
            CurrentTime = initialDateTime ?? DateTime.UtcNow;

            queue = new Queue<QueuedSagaMessage>();
            persister = new NonDurableSagaPersister();
            storedTimeouts = new List<(DateTime, OutgoingMessage<object, SendOptions>)>();
            handlerSimulations = new Dictionary<Type, List<(Type, Func<object, object>)>>();

            sagaMapper = SagaMapper.Get<TSaga, TSagaEntity>(this.sagaFactory);
        }

        /// <summary>
        /// Shows the "current time" of the saga test, which can be set in the constructor, or will default to the current time.
        /// As this is advanced using the <see cref="AdvanceTime"/> method, timeouts requested by earlier message handlers
        /// will be dispatched and the CurrentTime will be updated.
        /// </summary>
        public DateTime CurrentTime { get; private set; }

        /// <summary>
        /// True if there are any messages in the message queue.
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
        public QueuedSagaMessage QueuePeek() => queue.Peek();

        /// <summary>
        /// Have the saga process a message. This will use the <see cref="NServiceBus.Saga{TSagaData}.ConfigureHowToFindSaga(SagaPropertyMapper{TSagaData})"/> method
        /// to find the correct saga data based on correlation ID values from the message, and will dispatch the message to the correct handler method
        /// based on message type.
        /// </summary>
        /// <typeparam name="TMessageType">The type of the message.</typeparam>
        /// <param name="message">The message for the saga to process.</param>
        /// <param name="context">
        /// An optional <see cref="TestableMessageHandlerContext"/> to use while processing this message.
        /// If none is supplied one will be created and returned in the resulting <see cref="MessageProcessingResult"/>.
        /// </param>
        /// <param name="messageHeaders">
        /// Optional message headers to include with the message. Usualy not needed unless the
        /// <see cref="NServiceBus.Saga{TSagaData}.ConfigureHowToFindSaga(SagaPropertyMapper{TSagaData})"/> method uses header mappings.
        /// </param>
        /// <returns>
        /// Returns a <see cref="MessageProcessingResult"/> containing a snapshot of the saga data after the message is processed
        /// and the <see cref="TestableMessageHandlerContext"/> containing details about context operations that occurred in the message
        /// handler, which can be used for test assertions.
        /// </returns>
        public Task<MessageProcessingResult> Process<TMessageType>(TMessageType message, TestableMessageHandlerContext context = null, IReadOnlyDictionary<string, string> messageHeaders = null)
        {
            if (context == null)
            {
                context = new TestableMessageHandlerContext();
            }

            var queueMessage = new QueuedSagaMessage(typeof(TMessageType), message, messageHeaders);
            return InnerProcess(queueMessage, "Handle", context);
        }

        /// <summary>
        /// Have the saga process an auto-correlated message, such as a reply from a handler the saga sent a command to.
        /// </summary>
        /// <typeparam name="TMessageType">The type of the message.</typeparam>
        /// <param name="sagaId">The saga Id that would be embedded in the message headers, because it was sent by the saga, or is an auto-correlated reply to a message sent by the saga.</param>
        /// <param name="message">The message for the saga to process.</param>
        /// <param name="context">
        /// An optional <see cref="TestableMessageHandlerContext"/> to use while processing this message.
        /// If none is supplied one will be created and returned in the resulting <see cref="MessageProcessingResult"/>.
        /// </param>
        /// <param name="messageHeaders">
        /// Optional message headers to include with the message. Usualy not needed unless the
        /// <see cref="NServiceBus.Saga{TSagaData}.ConfigureHowToFindSaga(SagaPropertyMapper{TSagaData})"/> method uses header mappings.
        /// </param>
        /// <returns>
        /// Returns a <see cref="MessageProcessingResult"/> containing a snapshot of the saga data after the message is processed
        /// and the <see cref="TestableMessageHandlerContext"/> containing details about context operations that occurred in the message
        /// handler, which can be used for test assertions.
        /// </returns>
        public Task<MessageProcessingResult> ProcessAutoCorrelatedReply<TMessageType>(Guid sagaId, TMessageType message, TestableMessageHandlerContext context = null, IReadOnlyDictionary<string, string> messageHeaders = null)
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

            return Process(message, context, newHeaders);
        }

        /// <summary>
        /// Processes a queued message. Messages can be queued because the saga sends or publishes a message that is handled by this saga,
        /// or as a result of an automatically-generated reply message from using the <see cref="SimulateHandlerResponse"/> method.
        /// </summary>
        /// <param name="context">
        /// An optional <see cref="TestableMessageHandlerContext"/> to use while processing this message.
        /// If none is supplied one will be created and returned in the resulting <see cref="MessageProcessingResult"/>.
        /// </param>
        /// <returns>
        /// Returns a <see cref="MessageProcessingResult"/> containing a snapshot of the saga data after the message is processed
        /// and the <see cref="TestableMessageHandlerContext"/> containing details about context operations that occurred in the message
        /// handler, which can be used for test assertions.
        /// </returns>
        public Task<MessageProcessingResult> ProcessQueuedMessage(TestableMessageHandlerContext context = null)
        {
            if (!queue.Any())
            {
                throw new Exception("Could not process the next message out of the queue, because there are no queued messages.");
            }

            if (context == null)
            {
                context = new TestableMessageHandlerContext();
            }

            var queuedMessage = queue.Dequeue();
            return InnerProcess(queuedMessage, "Handle", context);
        }

        /// <summary>
        /// Simulates an external handler replying to a message sent from this saga when used in the commander style. The delegate will be used
        /// to create a response message, and it will automatically have headers that mark it as belonging to the correct saga data instance,
        /// which mimics how message handlers support auto-correlation when replying to messages from a saga.
        /// </summary>
        /// <typeparam name="TMessageFromSaga">The message type sent or published by the saga.</typeparam>
        /// <typeparam name="TResponseMessage">The message type that the external handler would reply with.</typeparam>
        /// <param name="responseGenerator">
        /// Generates the response message based on a message sent by the saga. Return null to simulate not replying to the message.
        /// </param>
        /// <exception cref="Exception"></exception>
        public void SimulateHandlerResponse<TMessageFromSaga, TResponseMessage>(Func<TMessageFromSaga, TResponseMessage> responseGenerator)
        {
            if (!sagaMapper.HandlesMessageType(typeof(TResponseMessage)))
            {
                throw new Exception($"Messages of type {typeof(TResponseMessage).FullName} are not handled by the saga.");
            }

            Func<object, object> wrapperFunc = messageObject => responseGenerator((TMessageFromSaga)messageObject);

            if (!handlerSimulations.TryGetValue(typeof(TMessageFromSaga), out var list))
            {
                list = new List<(Type, Func<object, object>)>();
                handlerSimulations.Add(typeof(TMessageFromSaga), list);
            }

            list.Add((typeof(TResponseMessage), wrapperFunc));
        }

        async Task<MessageProcessingResult> InnerProcess(QueuedSagaMessage message, string handleMethodName, TestableMessageHandlerContext context)
        {
            var saga = sagaFactory();

            using (var session = new NonDurableSynchronizedStorageSession())
            {
                var contextBag = new ContextBag();

                var loadResult = await LoadSagaData(message, session, contextBag, context.CancellationToken).ConfigureAwait(false);
                saga.Entity = loadResult.Data;

                await InvokeSagaHandler(saga, handleMethodName, message, context).ConfigureAwait(false);
                await SaveSagaData(saga, loadResult.IsNew, loadResult.MappedValue, session, contextBag, context.CancellationToken).ConfigureAwait(false);
                await session.CompleteAsync(context.CancellationToken).ConfigureAwait(false);
            }

            EnqueueMessagesAndTimeouts(context, saga.Entity.Id);

            return new MessageProcessingResult(saga, message, context);
        }

        Task InvokeSagaHandler(TSaga saga, string methodName, QueuedSagaMessage message, TestableMessageHandlerContext context)
        {
            return sagaMapper.InvokeHandlerMethod(saga, methodName, message, context);
        }

        /// <summary>
        /// Advance the <see cref="CurrentTime"/> for the test, dispatching any stored timeouts along the way.
        /// </summary>
        /// <param name="timeToAdvance">Amount of time to advance the <see cref="CurrentTime"/>.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>
        /// Returns an array of <see cref="MessageProcessingResult"/>, one for each timeout that was dispatched and processed
        /// during the time that was advanced. If no timeouts were fired, the array will be empty.
        /// </returns>
        public async Task<MessageProcessingResult[]> AdvanceTime(TimeSpan timeToAdvance, CancellationToken cancellationToken = default)
        {
            var advanceToTime = CurrentTime + timeToAdvance;

            var due = storedTimeouts.Where(t => t.At <= advanceToTime).OrderBy(t => t.At).ToArray();
            storedTimeouts.RemoveAll(t => t.At <= advanceToTime);

            var results = new List<MessageProcessingResult>();

            foreach (var storedTimeout in due)
            {
                CurrentTime = storedTimeout.At;
                var result = await ProcessTimeout(storedTimeout.Timeout, cancellationToken).ConfigureAwait(false);
                results.Add(result);
            }

            CurrentTime = advanceToTime;

            return results.ToArray();
        }

        async Task<MessageProcessingResult> ProcessTimeout(OutgoingMessage<object, SendOptions> timeoutMessage, CancellationToken cancellationToken)
        {
            var messageType = timeoutMessage.Message.GetType();
            var context = new TestableMessageHandlerContext { CancellationToken = cancellationToken };

            var queueMessage = new QueuedSagaMessage(messageType, timeoutMessage.Message, timeoutMessage.Options.GetHeaders());

            var methodName = (timeoutMessage is TimeoutMessage<object>) ? "Timeout" : "Handle";

            var processingResult = await InnerProcess(queueMessage, methodName, context).ConfigureAwait(false);

            return processingResult;
        }

        async Task<(TSagaEntity Data, bool IsNew, object MappedValue)> LoadSagaData(QueuedSagaMessage message, ISynchronizedStorageSession session, ContextBag contextBag, CancellationToken cancellationToken)
        {
            var messageMetadata = sagaMapper.GetMessageMetadata(message.Type);
            TSagaEntity sagaData;

            if (message.Headers != null && message.Headers.TryGetValue(Headers.SagaId, out var sagaIdString) && Guid.TryParse(sagaIdString, out Guid sagaId))
            {
                sagaData = await persister.Get<TSagaEntity>(sagaId, session, contextBag, cancellationToken).ConfigureAwait(false);
                if (sagaData != null)
                {
                    return (sagaData, false, null);
                }
            }

            var messageMappedValue = sagaMapper.GetMessageMappedValue(message);

            sagaData = await persister.Get<TSagaEntity>(sagaMapper.CorrelationPropertyName, messageMappedValue, session, contextBag, cancellationToken).ConfigureAwait(false);

            if (sagaData != null)
            {
                return (sagaData, false, messageMappedValue);
            }

            if (messageMetadata.IsAllowedToStartSaga)
            {
                sagaData = new TSagaEntity { Id = Guid.NewGuid() };
                sagaMapper.SetCorrelationPropertyValue(sagaData, messageMappedValue);
                return (sagaData, true, messageMappedValue);
            }

            throw new Exception($"Saga not found and message type {message.Type.FullName} is not allowed to start the saga.");
        }

        async Task SaveSagaData(TSaga saga, bool isNew, object mappedValue, ISynchronizedStorageSession session, ContextBag contextBag, CancellationToken cancellationToken)
        {
            if (saga.Completed)
            {
                await persister.Complete(saga.Entity, session, contextBag, cancellationToken).ConfigureAwait(false);
            }
            else if (isNew)
            {
                var saveCorrelationProperty = new SagaCorrelationProperty(sagaMapper.CorrelationPropertyName, mappedValue);
                await persister.Save(saga.Entity, saveCorrelationProperty, session, contextBag, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                await persister.Update(saga.Entity, session, contextBag, cancellationToken).ConfigureAwait(false);
            }
        }

        void EnqueueMessagesAndTimeouts(TestableMessageHandlerContext context, Guid sagaId)
        {
            foreach (var timeout in context.TimeoutMessages)
            {
                if (timeout.Within.HasValue)
                {
                    var targetTime = CurrentTime + timeout.Within.Value;
                    storedTimeouts.Add((targetTime, timeout));
                }
                else if (timeout.At.HasValue)
                {
                    storedTimeouts.Add((timeout.At.Value.UtcDateTime, timeout));
                }
                else
                {
                    throw new Exception("Unable to process a timeout message with no delivery time set. (This really shouldn't happen.)");
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
                        storedTimeouts.Add((targetTime, message));
                    }
                    else if (deliverAt.HasValue)
                    {
                        storedTimeouts.Add((deliverAt.Value.UtcDateTime, message));
                    }
                    else
                    {
                        var queuedMessage = new QueuedSagaMessage(msgType, message.Message, headers, sagaId);
                        queue.Enqueue(queuedMessage);
                    }
                }
                else if (handlerSimulations.TryGetValue(msgType, out var simulators))
                {
                    foreach (var simulator in simulators)
                    {
                        var newMessageObject = simulator.Item2(message.Message);
                        if (newMessageObject != null)
                        {
                            var queuedMessage = new QueuedSagaMessage(simulator.Item1, newMessageObject, autoCorrelatedSagaId: sagaId);
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
        /// The result of a saga processing a message.
        /// </summary>
        [DebuggerDisplay("MessageProcessingResult: {HandledMessage.Message}")]
        public class MessageProcessingResult
        {
            internal MessageProcessingResult(NServiceBus.Saga<TSagaEntity> saga, QueuedSagaMessage handledMessage, TestableMessageHandlerContext context)
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
            public Guid SagaId { get; private set; }
            /// <summary>
            /// True if the result of the message handler was that the <see cref="Saga.MarkAsComplete"/> method was called.
            /// </summary>
            public bool Completed { get; private set; }

            /// <summary>
            /// Details about the message that was just handled.
            /// </summary>
            public QueuedSagaMessage HandledMessage { get; private set; }

            /// <summary>
            /// The <see cref="IMessageHandlerContext"/> that was used to process the message, which contains details of
            /// what happened during the handler method that can be used for test assertions.
            /// </summary>
            public TestableMessageHandlerContext Context { get; private set; }
            /// <summary>
            /// A copy of the saga data after the message was processed.
            /// </summary>
            public TSagaEntity SagaDataSnapshot { get; private set; }

            /// <summary>
            /// Find the first published message of a given type. Returns null if no messages of that type are found.
            /// </summary>
            public TMessageType FindPublishedMessage<TMessageType>() => Context.FindPublishedMessage<TMessageType>();

            /// <summary>
            /// Find the first sent message of a given type. Returns null if no messages of that type are found.
            /// </summary>
            public TMessageType FindSentMessage<TMessageType>() => Context.FindSentMessage<TMessageType>();

            /// <summary>
            /// Find the first timeout message of a given type. Returns null if no messages of that type are found.
            /// </summary>
            public TMessageType FindTimeoutMessage<TMessageType>() => Context.FindTimeoutMessage<TMessageType>();

            /// <summary>
            /// Find the first reply message of a given type. Returns null if no messages of that type are found.
            /// </summary>
            public TMessageType FindReplyMessage<TMessageType>() => Context.FindReplyMessage<TMessageType>();
        }
    }
}
