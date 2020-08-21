// ReSharper disable PartialTypeWithSinglePart
namespace NServiceBus.Testing
{
    using System;
    using DependencyInjection;
    using Microsoft.Extensions.DependencyInjection;
    using MicrosoftExtensionsDependencyInjection;
    using Pipeline;

    /// <summary>
    /// Base implementation for contexts implementing <see cref="IIncomingContext" />.
    /// </summary>
    public abstract partial class TestableIncomingContext : TestableMessageProcessingContext, IIncomingContext
    {
        /// <summary>
        /// Creates a new instance of <see cref="TestableIncomingContext" />.
        /// </summary>
        protected TestableIncomingContext(IMessageCreator messageCreator = null) : base(messageCreator)
        {
        }

        /// <summary>
        /// The <see cref="IServiceCollection"/> to build an <see cref="IServiceProvider"/> once the <see cref="IBehaviorContext.Builder"/> is accessed. Override <see cref="GetBuilder" /> to customize the <see cref="IServiceProvider"/> implementation used.
        /// </summary>
        public IServiceCollection ServiceCollection { get; set; } = new ServiceCollection();

        IServiceProvider IBehaviorContext.Builder => GetBuilder();

        /// <summary>
        /// Selects the builder returned by <see cref="IBehaviorContext.Builder" />. Override this method to provide your custom <see cref="IServiceProvider" /> implementation.
        /// </summary>
        protected virtual IServiceProvider GetBuilder()
        {
            return ServiceCollection.BuildDefaultNServiceBusProvider();
        }
    }
}