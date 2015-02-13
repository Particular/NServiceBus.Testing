namespace NServiceBus.Testing
{
    using Transports;
    using Unicast;

    class FakeSender : ISendMessages
    {
        public void Send(TransportMessage message, SendOptions sendOptions)
        {

        }
    }
}