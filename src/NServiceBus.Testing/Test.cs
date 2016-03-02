namespace NServiceBus.Testing
{
    using System;
    using NServiceBus.MessageInterfaces.MessageMapper.Reflection;

    /// <summary>
    /// Entry class used for unit testing
    /// </summary>
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
        public static Saga<TSaga> Saga<TSaga>(TSaga saga) where TSaga : Saga, new()
        {
            return new Saga<TSaga>(saga);
        }

        /// <summary>
        /// Begin the test script for a saga of type T while specifying the saga id.
        /// </summary>
        public static Saga<T> Saga<T>(Guid sagaId) where T : Saga, new()
        {
            var saga = (T)Activator.CreateInstance(typeof(T));

            var prop = typeof(T).GetProperty("Data");

            if (prop != null)
            {
                var sagaData = Activator.CreateInstance(prop.PropertyType) as IContainSagaData;

                saga.Entity = sagaData;

                if (saga.Entity != null)
                {
                    saga.Entity.Id = sagaId;
                }
            }

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