#pragma warning disable 1591

namespace NServiceBus.Testing
{
    [ObsoleteEx(
        Message = "Forwarding functionality has been removed from NServiceBus.",
        RemoveInVersion = "9",
        TreatAsErrorFromVersion = "8")]
    public class TestableForwardingContext
    {
    }
}

#pragma warning restore 1591