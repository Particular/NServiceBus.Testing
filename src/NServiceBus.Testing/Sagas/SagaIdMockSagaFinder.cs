namespace NServiceBus.Testing;

using System;
using System.Threading;
using System.Threading.Tasks;
using Extensibility;
using Persistence;
using Sagas;

class SagaIdMockSagaFinder<TSagaData, TMessage>(ISagaPersister sagaPersister, Func<TMessage, Guid?> mockFinder)
    : ISagaFinder<TSagaData, TMessage>
    where TSagaData : class, IContainSagaData
{
    public Task<TSagaData> FindBy(TMessage message, ISynchronizedStorageSession storageSession,
        IReadOnlyContextBag context, CancellationToken cancellationToken = default)
    {
        var sagaId = mockFinder(message);
        var sagaData = sagaId == null
            ? Task.FromResult((TSagaData)null)
            : sagaPersister.Get<TSagaData>(sagaId.Value, storageSession, (ContextBag)context,
                cancellationToken);

        return sagaData;
    }
}