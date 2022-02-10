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

namespace NServiceBus.Testing
{
    [ObsoleteEx(
        Message = "Use the arrange act assert (AAA) syntax instead. Please see the upgrade guide for more details.",
        RemoveInVersion = "9",
        TreatAsErrorFromVersion = "8")]
    public class Saga<T>
    {
    }
}

namespace NServiceBus.Testing
{
    [ObsoleteEx(
        Message = "Use the arrange act assert (AAA) syntax instead. Please see the upgrade guide for more details.",
        RemoveInVersion = "9",
        TreatAsErrorFromVersion = "8")]
    public class Test
    {
    }
}

namespace NServiceBus.Testing
{
    using System;

    [ObsoleteEx(
        Message = "Use the arrange act assert (AAA) syntax instead. Please see the upgrade guide for more details.",
        RemoveInVersion = "9",
        TreatAsErrorFromVersion = "8")]
    public class ExpectationException : Exception
    {
    }
}

namespace NServiceBus.Testing
{
    [ObsoleteEx(
        Message = "Use the arrange act assert (AAA) syntax instead. Please see the upgrade guide for more details.",
        RemoveInVersion = "9",
        TreatAsErrorFromVersion = "8")]
    public class Handler<T>
    {
    }
}

namespace NServiceBus.Testing
{
    using System;

    public partial class TestableMessageSession
    {
        [ObsoleteEx(
            RemoveInVersion = "9",
            TreatAsErrorFromVersion = "8")]
        public Extensibility.ContextBag Extensions
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
    }
}


namespace NServiceBus.Testing
{
    using System.Collections.Generic;


    public partial class TestableAuditContext
    {
        [ObsoleteEx(
            ReplacementTypeOrMember = nameof(AuditMetadata),
            RemoveInVersion = "9",
            TreatAsErrorFromVersion = "8")]
        public void AddAuditData(string key, string value)
        {
            AuditMetadata.Add(key, value);
        }

        [ObsoleteEx(
            ReplacementTypeOrMember = nameof(AuditMetadata),
            RemoveInVersion = "9",
            TreatAsErrorFromVersion = "8")]
        public Dictionary<string, string> AddedAuditData { get; }
    }
}

#pragma warning restore 1591