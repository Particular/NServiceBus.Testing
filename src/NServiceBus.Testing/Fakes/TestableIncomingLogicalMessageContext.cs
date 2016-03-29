﻿namespace NServiceBus.Testing
{
    using System.Collections.Generic;
    using NServiceBus.Pipeline;

    /// <summary>
    /// A testable implementation of <see cref="IIncomingLogicalMessageContext" />.
    /// </summary>
    public class TestableIncomingLogicalMessageContext : TestableIncomingContext, IIncomingLogicalMessageContext
    {
        /// <summary>
        /// Creates a new instance of <see cref="TestableIncomingLogicalMessageContext" />.
        /// </summary>
        public TestableIncomingLogicalMessageContext(IMessageCreator messageCreator = null) : base(messageCreator)
        {
        }

        /// <summary>
        /// Message being handled.
        /// </summary>
        public LogicalMessage Message { get; set; }

        /// <summary>
        /// Headers for the incoming message.
        /// </summary>
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Tells if the message has been handled.
        /// </summary>
        public bool MessageHandled { get; set; }
    }
}