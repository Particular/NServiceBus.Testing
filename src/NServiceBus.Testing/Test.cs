namespace NServiceBus.Testing
{
    using System;
    using MessageInterfaces.MessageMapper.Reflection;

    /// <summary>
    /// Entry class used for unit testing
    /// </summary>
    [ObsoleteEx(
     Message = "Use the arrange act assert (AAA) syntax instead. Please see the upgrade guide for more details.",
     RemoveInVersion = "9",
     TreatAsErrorFromVersion = "8")]
    public class Test
    {
        /// <summary>
        /// Begin the test script for a saga of type T.
        /// </summary>
        public static Saga<TSaga> Saga<TSaga>() where TSaga : Saga, new()
        {
            return new Saga<TSaga>(new TSaga());
        }

        /// <summary>
        /// Begin the test script for the passed in saga instance.
        /// Callers need to instantiate the saga's data class as well as give it an ID.
        /// </summary>
        public static Saga<TSaga> Saga<TSaga>(TSaga saga) where TSaga : Saga
        {
            return new Saga<TSaga>(saga);
        }

        /// <summary>
        /// Begin the test script for a saga of type T while specifying the saga id.
        /// </summary>
        public static Saga<TSaga> Saga<TSaga>(Guid sagaId) where TSaga : Saga, new()
        {
            var prop = typeof(TSaga).GetProperty("Data");
            IContainSagaData sagaData = null;

            if (prop != null)
            {
                sagaData = (IContainSagaData)Activator.CreateInstance(prop.PropertyType);
                sagaData.Id = sagaId;
            }

            return Saga<TSaga>(sagaData);
        }

        /// <summary>
        /// Begin the test script for a saga of type T with the passed in in <see cref="IContainSagaData" />.
        /// </summary>
        public static Saga<TSaga> Saga<TSaga>(IContainSagaData sagaData) where TSaga : Saga, new()
        {
            var saga = new TSaga
            {
                Entity = sagaData
            };

            return Saga(saga);
        }

        /// <summary>
        /// Specify a test for a message handler of type T for a given message of type TMessage.
        /// </summary>
        public static Handler<THandler> Handler<THandler>() where THandler : new()
        {
            return Handler(new THandler());
        }

        /// <summary>
        /// Specify a test for a message handler while supplying the instance to
        /// test - injects the bus into a public property (if it exists).
        /// </summary>
        public static Handler<THandler> Handler<THandler>(THandler handler)
        {
            return new Handler<THandler>(handler);
        }

        /// <summary>
        /// Instantiate a new message of type TMessage.
        /// </summary>
        public static TMessage CreateInstance<TMessage>()
        {
            return messageCreator.CreateInstance<TMessage>();
        }

        /// <summary>
        /// Instantiate a new message of type TMessage performing the given action
        /// on the created message.
        /// </summary>
        public static TMessage CreateInstance<TMessage>(Action<TMessage> action)
        {
            return messageCreator.CreateInstance(action);
        }

        static IMessageCreator messageCreator = new MessageMapper();
    }
}