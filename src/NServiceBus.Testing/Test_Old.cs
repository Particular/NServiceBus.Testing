//namespace NServiceBus.Testing
//{
//    using System;
//    using System.Linq;
//    using DataBus.InMemory;
//    using Features;
//    using MessageInterfaces;

//    /// <summary>
//    ///     Entry class used for unit testing
//    /// </summary>
//    public static class Test
//    {
//        /// <summary>
//        ///     Initializes the testing infrastructure.
//        /// </summary>
//        public static void Initialize()
//        {
//            var configuration = new BusConfiguration();

//            configuration.EndpointName("UnitTests");
//            configuration.CustomConfigurationSource(testConfigurationSource);
//            configuration.DiscardFailedMessagesInsteadOfSendingToErrorQueue();
//            configuration.DisableFeature<Sagas>();
//            configuration.DisableFeature<Audit>();
//            configuration.UseTransport<FakeTestTransport>();
//            configuration.UsePersistence<InMemoryPersistence>();
//            configuration.RegisterEncryptionService(() => new FakeEncryptor());
//            configuration.RegisterComponents(r =>
//            {
//                r.ConfigureComponent<InMemoryDataBus>(DependencyLifecycle.SingleInstance);
//                r.ConfigureComponent<FakeQueueCreator>(DependencyLifecycle.InstancePerCall);
//                r.ConfigureComponent<FakeDequer>(DependencyLifecycle.InstancePerCall);
//                r.ConfigureComponent<FakeSender>(DependencyLifecycle.InstancePerCall);
//            });
            
//            if (initialized)
//            {
//                return;
//            }

//            var encpointInstance = Endpoint.Create(configuration);

//            var mapper = (encpointInstance).Build<IMessageMapper>();

//            messageCreator = mapper;

//            initialized = true;
//        } 

//        /// <summary>
//        ///     Begin the test script for a saga of type T.
//        /// </summary>
//        public static Saga<T> Saga<T>() where T : Saga, new()
//        {
//            return Saga<T>(Guid.NewGuid());
//        }

//        /// <summary>
//        ///     Begin the test script for a saga of type T while specifying the saga id.
//        /// </summary>
//        public static Saga<T> Saga<T>(Guid sagaId) where T : Saga, new()
//        {
//            var saga = (T)Activator.CreateInstance(typeof(T));

//            var prop = typeof(T).GetProperty("Data");

//            if (prop != null)
//            {
//                var sagaData = Activator.CreateInstance(prop.PropertyType) as IContainSagaData;

//                saga.Entity = sagaData;

//                if (saga.Entity != null)
//                {
//                    saga.Entity.Id = sagaId;
//                }
//            }

//            return Saga(saga);
//        }

//        /// <summary>
//        ///     Begin the test script for the passed in saga instance.
//        ///     Callers need to instantiate the saga's data class as well as give it an ID.
//        /// </summary>
//        public static Saga<T> Saga<T>(T saga) where T : Saga, new()
//        {
//            bus = new StubBus(messageCreator);

//            saga.Bus = Bus;

//            return new Saga<T>(saga, bus);
//        }

//        /// <summary>
//        ///     Specify a test for a message handler of type T for a given message of type TMessage.
//        /// </summary>
//        public static Handler<T> Handler<T>() where T : new()
//        {
//            var handler = (T)Activator.CreateInstance(typeof(T));

//            return Handler(handler);
//        }

//        /// <summary>
//        ///     Specify a test for a message handler while supplying the instance to
//        ///     test - injects the bus into a public property (if it exists).
//        /// </summary>
//        public static Handler<T> Handler<T>(T handler)
//        {
//            Func<IBus, T> handlerCreator = b => handler;
//            var prop = typeof(T).GetProperties().FirstOrDefault(p => p.PropertyType == typeof(IBus));
//            if (prop != null)
//            {
//                handlerCreator = b =>
//                {
//                    prop.SetValue(handler, b, null);
//                    return handler;
//                };
//            }

//            return Handler(handlerCreator);
//        }

//        /// <summary>
//        ///     Specify a test for a message handler specifying a callback to create
//        ///     the handler and getting an instance of the bus passed in.
//        ///     Useful for handlers based on constructor injection.
//        /// </summary>
//        public static Handler<T> Handler<T>(Func<IBus, T> handlerCreationCallback)
//        {
//            bus = new StubBus(messageCreator);

//            var handler = handlerCreationCallback.Invoke(bus);

//            var isHandler = (from i in handler.GetType().GetInterfaces()
//                             let args = i.GetGenericArguments()
//                             where args.Length == 1
//                             where typeof(IHandleMessages<>).MakeGenericType(args[0]).IsAssignableFrom(i)
//                             select i).Any();

//            if (!isHandler)
//            {
//                throw new ArgumentException("The handler object created does not implement IHandleMessages<T>.", "handlerCreationCallback");
//            }

//            return new Handler<T>(handler, bus);
//        }

//        /// <summary>
//        ///     Instantiate a new message of type TMessage.
//        /// </summary>
//        public static TMessage CreateInstance<TMessage>()
//        {
//            return messageCreator.CreateInstance<TMessage>();
//        }

//        /// <summary>
//        ///     Instantiate a new message of type TMessage performing the given action
//        ///     on the created message.
//        /// </summary>
//        public static TMessage CreateInstance<TMessage>(Action<TMessage> action)
//        {
//            return messageCreator.CreateInstance(action);
//        }

//        [ThreadStatic]
//        static StubBus bus;

//        static IMessageCreator messageCreator;
//        static readonly TestConfigurationSource testConfigurationSource = new TestConfigurationSource();
//        static bool initialized;
//    }
//}
