namespace NServiceBus.Testing
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Transactions;
    using Extensibility;
    using Outbox;
    using Persistence;
    using Transport;

    class NonDurableSynchronizedStorageSession : ICompletableSynchronizedStorageSession
    {
        public NonDurableTransaction Transaction { get; private set; }

        public void Dispose()
        {
            Transaction = null;
        }

        public ValueTask DisposeAsync()
        {
            Transaction = null;

            return ValueTask.CompletedTask;
        }

        public ValueTask<bool> TryOpen(IOutboxTransaction transaction, ContextBag context, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException("Outbox transactions are not supported in the testing framework.");
        }

        public ValueTask<bool> TryOpen(TransportTransaction transportTransaction, ContextBag context, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException("Transport transactions are not supported in the testing framework.");
        }

        public Task Open(ContextBag contextBag, CancellationToken cancellationToken = new CancellationToken())
        {
            Transaction = new NonDurableTransaction();
            ownsTransaction = true;
            return Task.CompletedTask;
        }

        public Task CompleteAsync(CancellationToken cancellationToken = default)
        {
            if (ownsTransaction)
            {
                Transaction.Commit();
            }
            return Task.CompletedTask;
        }

        public void Enlist(Action action) => Transaction.Enlist(action);

        bool ownsTransaction;

        class EnlistmentNotification2 : IEnlistmentNotification
        {
            public EnlistmentNotification2(NonDurableTransaction transaction) => this.transaction = transaction;

            public void Prepare(PreparingEnlistment preparingEnlistment)
            {
                try
                {
                    transaction.Commit();
                    preparingEnlistment.Prepared();
                }
                catch (Exception ex)
                {
                    preparingEnlistment.ForceRollback(ex);
                }
            }

            public void Commit(Enlistment enlistment) => enlistment.Done();

            public void Rollback(Enlistment enlistment)
            {
                transaction.Rollback();
                enlistment.Done();
            }

            public void InDoubt(Enlistment enlistment) => enlistment.Done();

            readonly NonDurableTransaction transaction;
        }
    }
}