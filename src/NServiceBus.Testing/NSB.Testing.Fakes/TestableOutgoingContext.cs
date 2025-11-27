namespace NServiceBus.Testing;

using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Pipeline;

/// <summary>
/// A base implementation for all behaviors implementing <see cref="IOutgoingContext" />.
/// </summary>
public partial class TestableOutgoingContext : TestablePipelineContext, IOutgoingContext
{
    /// <summary>
    /// The <see cref="IServiceCollection"/> to build an <see cref="IServiceProvider"/> once the <see cref="IBehaviorContext.Builder"/> is accessed. Override <see cref="GetBuilder" /> to customize the <see cref="IServiceProvider"/> implementation used.
    /// </summary>
    public IServiceCollection ServiceCollection { get; set; } = new ServiceCollection();

    IServiceProvider IBehaviorContext.Builder => GetBuilder();

    /// <summary>
    /// The id of the outgoing message.
    /// </summary>
    public string MessageId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// The headers of the outgoing message.
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = [];

    /// <summary>
    /// Selects the builder returned by <see cref="IBehaviorContext.Builder" />. Override this method to provide your custom <see cref="IServiceProvider" /> implementation.
    /// </summary>
    protected virtual IServiceProvider GetBuilder() => ServiceCollection.BuildServiceProvider();
}