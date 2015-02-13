namespace NServiceBus.Testing
{
    using System;
    using Transports;
    using Unicast.Transport;

    class FakeDequer : IDequeueMessages
    {
        public void Init(Address address, TransactionSettings transactionSettings, Func<TransportMessage, bool> tryProcessMessage, Action<TransportMessage, Exception> endProcessMessage)
        {

        }

        public void Start(int maximumConcurrencyLevel)
        {

        }

        public void Stop()
        {

        }
    }
}