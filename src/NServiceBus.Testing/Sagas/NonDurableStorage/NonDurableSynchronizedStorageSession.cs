namespace NServiceBus.Testing
{
    using System;
    using System.Threading.Tasks;
    using Persistence;

    class NonDurableSynchronizedStorageSession : CompletableSynchronizedStorageSession
    {
        public NonDurableSynchronizedStorageSession(NonDurableTransaction transaction)
        {
            Transaction = transaction;
        }

        public NonDurableSynchronizedStorageSession()
            : this(new NonDurableTransaction())
        {
            ownsTransaction = true;
        }

        public NonDurableTransaction Transaction { get; private set; }

        public void Dispose()
        {
            Transaction = null;
        }

        public Task CompleteAsync()
        {
            if (ownsTransaction)
            {
                Transaction.Commit();
            }
            return Task.FromResult(0);
        }

        public void Enlist(Action action)
        {
            Transaction.Enlist(action);
        }

        readonly bool ownsTransaction;
    }
}