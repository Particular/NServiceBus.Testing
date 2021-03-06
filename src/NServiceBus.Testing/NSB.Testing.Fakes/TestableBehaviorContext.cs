﻿namespace NServiceBus.Testing
{
    using System;
    using System.Threading;
    using Extensibility;
    using Microsoft.Extensions.DependencyInjection;
    using Pipeline;

    /// <summary>
    /// A base implementation for contexts implementing <see cref="IBehaviorContext" />.
    /// </summary>
    public abstract partial class TestableBehaviorContext : IBehaviorContext
    {
        /// <summary>
        /// The <see cref="IServiceCollection"/> to build an <see cref="IServiceProvider"/> once the <see cref="IBehaviorContext.Builder"/> is accessed. Override <see cref="GetBuilder" /> to customize the <see cref="IServiceProvider"/> implementation used.
        /// </summary>
        public IServiceCollection ServiceCollection { get; set; } = new ServiceCollection();

        /// <summary>
        /// A <see cref="T:NServiceBus.Extensibility.ContextBag" /> which can be used to extend the current object.
        /// </summary>
        public ContextBag Extensions { get; set; } = new ContextBag();

        /// <summary>
        /// A <see cref="CancellationToken"/> to observe during message processing. Use this to verify behavior when message processing is canceled.
        /// </summary>
#pragma warning disable PS0002 // Instance methods on types implementing ICancellableContext should not have a CancellationToken parameter
        public CancellationToken CancellationToken { get; set; }
#pragma warning restore PS0002 // Instance methods on types implementing ICancellableContext should not have a CancellationToken parameter

        IServiceProvider IBehaviorContext.Builder => GetBuilder();

        /// <summary>
        /// Selects the builder returned by <see cref="IBehaviorContext.Builder" />. Override this method to provide your custom <see cref="IServiceProvider" /> implementation.
        /// </summary>
        protected virtual IServiceProvider GetBuilder()
        {
            return ServiceCollection.BuildServiceProvider();
        }
    }
}
