// ReSharper disable PartialTypeWithSinglePart
#pragma warning disable CS0618 // Type or member is obsolete
namespace NServiceBus.Testing
{
    using System;
    using System.Collections.Generic;
    using Pipeline;
    using Transport;

    /// <summary>
    /// A testable implementation for <see cref="IForwardingContext" />.
    /// </summary>
    [ObsoleteEx(TreatAsErrorFromVersion = "8.0", RemoveInVersion = "9.0")]
    public partial class TestableForwardingContext : TestableBehaviorContext, IForwardingContext
    {
        /// <summary>
        /// The message to be forwarded.
        /// </summary>
        public OutgoingMessage Message { get; set; } = new OutgoingMessage(Guid.NewGuid().ToString(), new Dictionary<string, string>(), new byte[0]);

        /// <summary>
        /// The address of the forwarding queue.
        /// </summary>
        public string Address { get; set; } = string.Empty;
    }
}
#pragma warning restore CS0618 // Type or member is obsolete