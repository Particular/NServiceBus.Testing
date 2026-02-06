namespace NServiceBus.Testing;

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
    static readonly ConcurrentDictionary<Type, SagaMapper> sagaMappers = new();

    readonly SagaMetadata metadata;
    readonly IReadOnlyDictionary<Type, SagaMapping> mappings;
    readonly SagaMetadata.CorrelationPropertyMetadata correlationProperty;
    readonly PropertyInfo correlationPropertyInfo;
    readonly ConcurrentDictionary<(Type messageType, string methodName), MethodInfo> handlerMethods;

    public string CorrelationPropertyName => correlationProperty.Name;

    SagaMapper(SagaMetadata sagaMetadata, object dummySagaForReflection)
    {
        metadata = sagaMetadata;
        handlerMethods = new ConcurrentDictionary<(Type messageType, string methodName), MethodInfo>();

        if (!metadata.TryGetCorrelationProperty(out correlationProperty))
        {
            throw new Exception($"Could not test saga {sagaMetadata.Name} because the correlation property could not be determined.");
        }

        correlationPropertyInfo = sagaMetadata.SagaEntityType.GetProperty(correlationProperty.Name);
        if (correlationPropertyInfo == null)
        {
            throw new Exception($"Could not test saga {sagaMetadata.Name} because a property on {sagaMetadata.Name} matching the correlation property '{correlationProperty.Name}' could not be located.");
        }

        var configureHowToFindMethod = sagaMetadata.SagaType.GetMethod("ConfigureHowToFindSaga", BindingFlags.NonPublic | BindingFlags.Instance, null, [typeof(IConfigureHowToFindSagaWithMessage)], null);
        if (configureHowToFindMethod == null)
        {
            throw new Exception($"Could not test saga {sagaMetadata.Name} because the ConfigureHowToFindSaga method could not be located.");
        }

        var mappingReader = new MappingReader();
        configureHowToFindMethod.Invoke(dummySagaForReflection, [mappingReader]);

        mappings = mappingReader.GetMappings();
    }

    public static SagaMapper Get<TSaga, TSagaEntity>(Func<TSaga> sagaFactory) where TSaga : Saga =>
        sagaMappers.GetOrAdd(typeof(TSaga), static (_, factory) =>
        {
            var dummySagaForReflection = factory();
            return new SagaMapper(SagaMetadata.Create<TSaga>(), dummySagaForReflection);
        }, sagaFactory);

    public bool HandlesMessageType(Type messageType)
        => metadata.AssociatedMessages.Any(m => m.MessageType == messageType);

    public SagaMessage GetMessageMetadata(Type messageType)
        => metadata.AssociatedMessages.FirstOrDefault(sagaMsg => messageType == sagaMsg.MessageType);

    public object GetMessageMappedValue(QueuedSagaMessage message)
    {
        if (!mappings.TryGetValue(message.Type, out var mapping))
        {
            throw new Exception("No mapped value found from message, could not look up saga data.");
        }

        return mapping.IsCustomFinder ? throw new NotSupportedException("Testing saga invocations with a custom saga finder is currently not supported") : mapping.Map(message);
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

        var invokeTask = handlerMethodInfo.Invoke(saga, [message.Message, context]) as Task;
        return invokeTask;
    }

    class MappingReader : IConfigureHowToFindSagaWithMessage, IConfigureHowToFindSagaWithMessageHeaders, IConfigureHowToFindSagaWithFinder
    {
        readonly Dictionary<Type, SagaMapping> mappings = [];

        void IConfigureHowToFindSagaWithMessage.ConfigureMapping<TSagaEntity, TMessage>(Expression<Func<TSagaEntity, object>> sagaEntityProperty, Expression<Func<TMessage, object>> messageProperty)
        {
            Func<TMessage, object> compiledExpression = messageProperty.Compile();
            object GetValueFromMessage(QueuedSagaMessage message) => compiledExpression((TMessage)message.Message);
            mappings.Add(typeof(TMessage), new SagaMapping(GetValueFromMessage, false));
        }

        void IConfigureHowToFindSagaWithMessageHeaders.ConfigureMapping<TSagaEntity, TMessage>(Expression<Func<TSagaEntity, object>> sagaEntityProperty, string headerName)
        {
            object GetValueFromMessage(QueuedSagaMessage message)
                => message.Headers.GetValueOrDefault(headerName);

            mappings.Add(typeof(TMessage), new SagaMapping(GetValueFromMessage, false));
        }

        void IConfigureHowToFindSagaWithFinder.ConfigureMapping<TSagaEntity, TMessage, TFinder>() => mappings.Add(typeof(TMessage), new SagaMapping(null, true));

        public IReadOnlyDictionary<Type, SagaMapping> GetMappings() =>
            new ReadOnlyDictionary<Type, SagaMapping>(mappings);
    }

    record SagaMapping(Func<QueuedSagaMessage, object> Map, bool IsCustomFinder);
}