namespace NServiceBus.Testing
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;
    using NServiceBus.Sagas;

    class SagaMapper
    {
        static readonly ConcurrentDictionary<Type, SagaMapper> sagaMappers = new ConcurrentDictionary<Type, SagaMapper>();

        readonly SagaMetadata metadata;
        readonly IReadOnlyDictionary<Type, Func<QueuedSagaMessage, object>> mappings;
        readonly SagaMetadata.CorrelationPropertyMetadata correlationProperty;
        readonly PropertyInfo correlationPropertyInfo;
        readonly ConcurrentDictionary<(Type messageType, string methodName), MethodInfo> handlerMethods;

        public string CorrelationPropertyName => correlationProperty.Name;

        SagaMapper(Type sagaType, Type sagaDataType, object dummySagaForReflection)
        {
            metadata = SagaMetadata.Create(sagaType);
            handlerMethods = new ConcurrentDictionary<(Type messageType, string methodName), MethodInfo>();

            if (!metadata.TryGetCorrelationProperty(out correlationProperty))
            {
                throw new Exception($"Could not test saga {sagaType.Name} because the correlation property could not be determined.");
            }

            correlationPropertyInfo = sagaDataType.GetProperty(correlationProperty.Name);
            if (correlationPropertyInfo == null)
            {
                throw new Exception($"Could not test saga {sagaType.Name} because a property on {sagaType.Name} matching the correlation property '{correlationProperty.Name}' could not be located.");
            }

            var configureHowToFindMethod = sagaType.GetMethod("ConfigureHowToFindSaga", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(IConfigureHowToFindSagaWithMessage) }, null);
            if (configureHowToFindMethod == null)
            {
                throw new Exception($"Could not test saga {sagaType.Name} because the ConfigureHowToFindSaga method could not be located.");
            }

            var mappingReader = new MappingReader();
            configureHowToFindMethod.Invoke(dummySagaForReflection, new object[] { mappingReader });

            mappings = mappingReader.GetMappings();
        }

        public static SagaMapper Get<TSaga, TSagaEntity>(Func<TSaga> sagaFactory)
        {
            return sagaMappers.GetOrAdd(typeof(TSaga), (sagaType, factory) =>
            {
                var dummySagaForReflection = factory();
                return new SagaMapper(typeof(TSaga), typeof(TSagaEntity), dummySagaForReflection);
            }, sagaFactory);
        }

        public bool HandlesMessageType(Type messageType)
            => metadata.AssociatedMessages.Any(m => m.MessageType == messageType);

        public SagaMessage GetMessageMetadata(Type messageType)
            => metadata.AssociatedMessages.FirstOrDefault(sagaMsg => messageType == sagaMsg.MessageType);

        public object GetMessageMappedValue(QueuedSagaMessage message)
        {
            if (mappings.TryGetValue(message.Type, out var mapping))
            {
                return mapping(message);
            }

            throw new Exception("No mapped value found from message, could not look up saga data.");
        }

        public void SetCorrelationPropertyValue(IContainSagaData sagaEntity, object value)
            => correlationPropertyInfo.SetValue(sagaEntity, value);

        public Task InvokeHandlerMethod<TSaga>(TSaga saga, string methodName, QueuedSagaMessage message, TestableMessageHandlerContext context)
        {
            var key = (message.Type, methodName);
            var handlerMethodInfo = handlerMethods.GetOrAdd(key, newKey =>
            {
                var handlerTypes = new Type[] { newKey.messageType, typeof(IMessageHandlerContext) };
                return typeof(TSaga).GetMethod(newKey.methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, handlerTypes, null);
            });

            var invokeTask = handlerMethodInfo.Invoke(saga, new object[] { message.Message, context }) as Task;
            return invokeTask;
        }

        class MappingReader : IConfigureHowToFindSagaWithMessage, IConfigureHowToFindSagaWithMessageHeaders
        {
            readonly Dictionary<Type, Func<QueuedSagaMessage, object>> mappings = new Dictionary<Type, Func<QueuedSagaMessage, object>>();

            void IConfigureHowToFindSagaWithMessage.ConfigureMapping<TSagaEntity, TMessage>(Expression<Func<TSagaEntity, object>> sagaEntityProperty, Expression<Func<TMessage, object>> messageProperty)
            {
                Func<TMessage, object> compiledExpression = messageProperty.Compile();
                object GetValueFromMessage(QueuedSagaMessage message) => compiledExpression((TMessage)message.Message);
                mappings.Add(typeof(TMessage), message => GetValueFromMessage(message));
            }

            void IConfigureHowToFindSagaWithMessageHeaders.ConfigureMapping<TSagaEntity, TMessage>(Expression<Func<TSagaEntity, object>> sagaEntityProperty, string headerName)
            {
                object GetValueFromMessage(QueuedSagaMessage message)
                    => message.Headers.TryGetValue(headerName, out var value) ? value : null;

                mappings.Add(typeof(TMessage), message => GetValueFromMessage(message));
            }

            public IReadOnlyDictionary<Type, Func<QueuedSagaMessage, object>> GetMappings() =>
                new ReadOnlyDictionary<Type, Func<QueuedSagaMessage, object>>(mappings);
        }
    }
}
