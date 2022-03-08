namespace NServiceBus.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Threading.Tasks;
    using NServiceBus.Sagas;

    class SagaMapper : IConfigureHowToFindSagaWithMessage, IConfigureHowToFindSagaWithMessageHeaders
    {
        static readonly Dictionary<Type, SagaMapper> sagaMappers = new Dictionary<Type, SagaMapper>();

        readonly SagaMetadata metadata;
        readonly Dictionary<Type, Func<QueuedSagaMessage, object>> mappings;
        readonly SagaMetadata.CorrelationPropertyMetadata correlationProperty;
        readonly PropertyInfo correlationPropertyInfo;
        readonly Dictionary<Tuple<Type, string>, MethodInfo> handlerMethods;

        public string CorrelationPropertyName => correlationProperty.Name;

        SagaMapper(Type sagaType, Type sagaDataType, object dummySagaForReflection)
        {
            metadata = SagaMetadata.Create(sagaType);
            mappings = new Dictionary<Type, Func<QueuedSagaMessage, object>>();
            handlerMethods = new Dictionary<Tuple<Type, string>, MethodInfo>();

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

            configureHowToFindMethod.Invoke(dummySagaForReflection, new object[] { this });
        }

        public static SagaMapper Get<TSaga, TSagaEntity>(Func<TSaga> sagaFactory)
        {
            if (!sagaMappers.TryGetValue(typeof(TSaga), out var sagaMapper))
            {
                var dummySagaForReflection = sagaFactory();
                sagaMapper = new SagaMapper(typeof(TSaga), typeof(TSagaEntity), dummySagaForReflection);
                sagaMappers[typeof(TSaga)] = sagaMapper;
            }

            return sagaMapper;
        }

        public bool HandlesMessageType(Type messageType)
        {
            return metadata.AssociatedMessages.Any(m => m.MessageType == messageType);
        }

        public SagaMessage GetMessageMetadata(Type messageType)
        {
            return metadata.AssociatedMessages.FirstOrDefault(sagaMsg => messageType == sagaMsg.MessageType);
        }

        public object GetMessageMappedValue(QueuedSagaMessage message)
        {
            if (mappings.TryGetValue(message.Type, out var mapping))
            {
                return mapping(message);
            }

            throw new Exception("No mapped value found from message, could not look up saga data.");
        }

        public void SetCorrelationPropertyValue(IContainSagaData sagaEntity, object value)
        {
            correlationPropertyInfo.SetValue(sagaEntity, value);
        }

        public Task InvokeHandlerMethod<TSaga>(TSaga saga, string methodName, QueuedSagaMessage message, TestableMessageHandlerContext context)
        {
            var key = new Tuple<Type, string>(message.Type, methodName);
            if (!handlerMethods.TryGetValue(key, out var handlerMethodInfo))
            {
                var handlerTypes = new Type[] { message.Type, typeof(IMessageHandlerContext) };
                handlerMethodInfo = typeof(TSaga).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, handlerTypes, null);
                handlerMethods[key] = handlerMethodInfo;
            }
            var invokeTask = handlerMethodInfo.Invoke(saga, new object[] { message.Message, context }) as Task;
            return invokeTask;
        }

        void IConfigureHowToFindSagaWithMessage.ConfigureMapping<TSagaEntity, TMessage>(Expression<Func<TSagaEntity, object>> sagaEntityProperty, Expression<Func<TMessage, object>> messageProperty)
        {
            Func<TMessage, object> compiledExpression = messageProperty.Compile();
            Func<QueuedSagaMessage, object> getValueFromMessage = message => compiledExpression((TMessage)message.Message);
            mappings.Add(typeof(TMessage), getValueFromMessage);
        }

        void IConfigureHowToFindSagaWithMessageHeaders.ConfigureMapping<TSagaEntity, TMessage>(Expression<Func<TSagaEntity, object>> sagaEntityProperty, string headerName)
        {
            Func<QueuedSagaMessage, object> getValueFromMessage = message =>
            {
                if (message.Headers.TryGetValue(headerName, out var value))
                {
                    return value;
                }
                return null;
            };
            mappings.Add(typeof(TMessage), getValueFromMessage);
        }
    }
}
