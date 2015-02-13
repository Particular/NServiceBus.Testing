namespace NServiceBus.Testing
{
    using Transports;

    class FakeTestTransport : TransportDefinition
    {
        protected override void Configure(BusConfiguration config)
        {
            config.EnableFeature<FakeTestTransportConfigurer>();
        }
    }
}