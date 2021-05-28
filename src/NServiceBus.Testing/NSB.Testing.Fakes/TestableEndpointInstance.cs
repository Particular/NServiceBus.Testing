namespace NServiceBus.Testing
{
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// A testable implementation of <see cref="IEndpointInstance" />.
    /// </summary>
    public partial class TestableEndpointInstance : TestableMessageSession, IEndpointInstance
    {
        /// <summary>
        /// Indicates whether <see cref="Stop" /> has been called or not.
        /// </summary>
        public bool EndpointStopped { get; private set; }

        /// <summary>
        /// Stops the endpoint.
        /// </summary>
#pragma warning disable PS0002 // Instance methods on types implementing ICancellableContext should not have a CancellationToken parameter
        public virtual Task Stop(CancellationToken cancellationToken = default)
#pragma warning restore PS0002 // Instance methods on types implementing ICancellableContext should not have a CancellationToken parameter
        {
            EndpointStopped = true;
            return Task.CompletedTask;
        }
    }
}