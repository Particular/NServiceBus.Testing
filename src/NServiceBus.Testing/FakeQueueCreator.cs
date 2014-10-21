namespace NServiceBus.Testing
{
    using Transports;

    class FakeQueueCreator : ICreateQueues
    {
        public void CreateQueueIfNecessary(Address address, string account)
        {
            //no-op
        }
    }
}