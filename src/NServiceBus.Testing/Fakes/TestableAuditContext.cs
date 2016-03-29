﻿namespace NServiceBus.Testing
{
    using System.Collections.Generic;
    using NServiceBus.Pipeline;
    using NServiceBus.Transports;

    /// <summary>
    /// A testable implementation of <see cref="IAuditContext" />.
    /// </summary>
    public class TestableAuditContext : TestableBehaviorContext, IAuditContext
    {
        /// <summary>
        /// Contains the information added by <see cref="AddedAuditData" />.
        /// </summary>
        public IDictionary<string, string> AddedAuditData { get; } = new Dictionary<string, string>();

        /// <summary>
        /// Address of the audit queue.
        /// </summary>
        public string AuditAddress { get; set; }

        /// <summary>
        /// The message to be audited.
        /// </summary>
        public OutgoingMessage Message { get; set; }

        /// <summary>
        /// Adds information about the current message that should be audited.
        /// </summary>
        /// <param name="key">The audit key.</param>
        /// <param name="value">The value.</param>
        public void AddAuditData(string key, string value)
        {
            AddedAuditData.Add(key, value);
        }
    }
}