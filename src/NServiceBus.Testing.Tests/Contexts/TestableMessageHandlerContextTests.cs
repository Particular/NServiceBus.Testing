namespace NServiceBus.Testing.Tests.Contexts;

using System;
using System.Collections.Generic;
using NUnit.Framework;

[TestFixture]
public class TestableMessageHandlerContextTests
{
    [Test]
    public void ProvidesDefaultHeadersThroughHandlerInterface()
    {
        IMessageHandlerContext context = new TestableMessageHandlerContext();

        Assert.Multiple(() =>
        {
            Assert.That(context.MessageId, Is.EqualTo("00000000-0000-0000-0000-000000000001"));
            Assert.That(context.MessageHeaders[Headers.MessageId], Is.EqualTo(context.MessageId));
            Assert.That(context.MessageHeaders[Headers.CorrelationId], Is.EqualTo("00000000-0000-0000-0000-000000000002"));
            Assert.That(context.MessageHeaders[Headers.ConversationId], Is.EqualTo("00000000-0000-0000-0000-000000000003"));
            Assert.That(context.MessageHeaders[Headers.ReplyToAddress], Is.EqualTo(context.ReplyToAddress));
            Assert.That(context.MessageHeaders[Headers.OriginatingEndpoint], Is.EqualTo("originating endpoint"));
            Assert.That(DateTimeOffsetHelper.ToDateTimeOffset(context.MessageHeaders[Headers.TimeSent]),
                Is.EqualTo(new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero)));
        });
    }

    [Test]
    public void DefaultsAreDeterministicAndIndependent()
    {
        var first = new TestableMessageHandlerContext();
        var second = new TestableMessageHandlerContext();

        Assert.That(first.MessageHeaders, Is.EquivalentTo(second.MessageHeaders));

        first.MessageHeaders[Headers.ConversationId] = "custom conversation";

        Assert.That(second.MessageHeaders[Headers.ConversationId], Is.EqualTo("00000000-0000-0000-0000-000000000003"));

        first.MessageHeaders.Clear();

        Assert.That(((IMessageHandlerContext)first).MessageHeaders, Is.Empty);
        Assert.That(second.MessageHeaders, Has.Count.EqualTo(6));
    }

    [Test]
    public void DefaultHeadersCanBeOverriddenAndExtended()
    {
        var timeSent = DateTimeOffsetHelper.ToWireFormattedString(new DateTimeOffset(2025, 6, 1, 12, 0, 0, TimeSpan.Zero));
        var context = new TestableMessageHandlerContext
        {
            MessageHeaders =
            {
                [Headers.ConversationId] = "custom conversation",
                [Headers.TimeSent] = timeSent,
                ["custom header"] = "custom value"
            }
        };
        IMessageHandlerContext handlerContext = context;

        Assert.Multiple(() =>
        {
            Assert.That(handlerContext.MessageHeaders[Headers.ConversationId], Is.EqualTo("custom conversation"));
            Assert.That(handlerContext.MessageHeaders[Headers.TimeSent], Is.EqualTo(timeSent));
            Assert.That(handlerContext.MessageHeaders["custom header"], Is.EqualTo("custom value"));
            Assert.That(handlerContext.MessageHeaders, Has.Count.EqualTo(7));
        });
    }

    [Test]
    public void DefaultHeadersCanBeReplaced()
    {
        var headers = new Dictionary<string, string> { ["custom header"] = "custom value" };
        var context = new TestableMessageHandlerContext { MessageHeaders = headers };
        IMessageHandlerContext handlerContext = context;

        Assert.That(context.MessageHeaders, Is.SameAs(headers));
        Assert.That(handlerContext.MessageHeaders, Is.EquivalentTo(headers));
    }
}
