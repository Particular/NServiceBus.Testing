namespace NServiceBus.Testing;

using System;
using System.Threading;
using System.Threading.Tasks;
using Extensibility;
using Persistence;
using Sagas;

class PropertyNameAndValueMockSagaFinder<TSagaData, TMessage>(ISagaPersister sagaPersister, Func<TMessage, (string propertyName, object propertyValue)> mockFinder)
    : ISagaFinder<TSagaData, TMessage>
    where TSagaData : class, IContainSagaData
{
    public Task<TSagaData> FindBy(TMessage message, ISynchronizedStorageSession storageSession,
        IReadOnlyContextBag context, CancellationToken cancellationToken = default)
    {
        var (propertyName, propertyValue) = mockFinder(message);
        return sagaPersister.Get<TSagaData>(propertyName, propertyValue, storageSession, (ContextBag)context,
            cancellationToken);
    }
}