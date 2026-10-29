namespace NServiceBus.Testing;

/// <summary>
/// A testable implementation of <see cref="IMessageHandlerContext" />.
/// </summary>
public partial class TestableMessageHandlerContext : TestableInvokeHandlerContext
{
    /// <summary>
    /// Creates a new instance of a <see cref="TestableMessageHandlerContext" /> with deterministic default message headers.
    /// </summary>
    public TestableMessageHandlerContext(IMessageCreator messageCreator = null) : base(messageCreator)
    {
    }
}