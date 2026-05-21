namespace NServiceBus.Testing;

using System;
using System.Threading;
using System.Threading.Tasks;
using Particular.Obsoletes;

/// <summary>
/// A testable implementation of <see cref="IEndpointInstance" />.
/// </summary>
[ObsoleteMetadata(
    Message = "This is a testable implementation of a type that is being removed from NServiceBus.",
    TreatAsErrorFromVersion = "11",
    RemoveInVersion = "12")]
[Obsolete("This is a testable implementation of a type that is being removed from NServiceBus. Will be treated as an error from version 11.0.0. Will be removed in version 12.0.0.", false)]
public partial class TestableEndpointInstance : TestableMessageSession, IEndpointInstance
{
    /// <summary>
    /// Indicates whether <see cref="Stop" /> has been called or not.
    /// </summary>
    public bool EndpointStopped { get; private set; }

    /// <summary>
    /// Stops the endpoint.
    /// </summary>
    public virtual Task Stop(CancellationToken cancellationToken = default)
    {
        EndpointStopped = true;
        return Task.CompletedTask;
    }
}