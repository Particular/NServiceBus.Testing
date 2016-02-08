namespace NServiceBus.Testing
{
    using System.Threading.Tasks;
    using NServiceBus.Extensibility;
    using Transports;

    class FakeSender : IDispatchMessages
    {
        public Task Dispatch(TransportOperations outgoingMessages, ContextBag context)
        {
            return Task.FromResult(0);
        }
    }
}