namespace NServiceBus.Testing.Tests.Contexts;

using System;
using System.Collections.Generic;
using NServiceBus.Pipeline;
using NServiceBus.Transport;
using NUnit.Framework;

[TestFixture]
public class DefaultMessageHeadersTests
{
    [TestCaseSource(nameof(HeaderSources))]
    public void ProvidesDefaultHeaders(Func<IDictionary<string, string>> createHeaders)
    {
        var headers = createHeaders();

        Assert.Multiple(() =>
        {
            Assert.That(headers, Has.Count.EqualTo(6));
            Assert.That(headers[Headers.MessageId], Is.EqualTo("00000000-0000-0000-0000-000000000001"));
            Assert.That(headers[Headers.CorrelationId], Is.EqualTo("00000000-0000-0000-0000-000000000002"));
            Assert.That(headers[Headers.ConversationId], Is.EqualTo("00000000-0000-0000-0000-000000000003"));
            Assert.That(headers[Headers.ReplyToAddress], Is.EqualTo("reply address"));
            Assert.That(headers[Headers.OriginatingEndpoint], Is.EqualTo("originating endpoint"));
            Assert.That(DateTimeOffsetHelper.ToDateTimeOffset(headers[Headers.TimeSent]),
                Is.EqualTo(new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero)));
        });
    }

    [TestCaseSource(nameof(HeaderSources))]
    public void DefaultsAreDeterministicAndIndependent(Func<IDictionary<string, string>> createHeaders)
    {
        var first = createHeaders();
        var second = createHeaders();

        Assert.That(first, Is.EquivalentTo(second));

        first[Headers.ConversationId] = "custom conversation";

        Assert.That(second[Headers.ConversationId], Is.EqualTo("00000000-0000-0000-0000-000000000003"));

        first.Clear();

        Assert.That(first, Is.Empty);
        Assert.That(second, Has.Count.EqualTo(6));
    }

    [TestCaseSource(nameof(ProcessingContexts))]
    public void SettingPropertiesUpdatesHeaders(Func<TestableMessageProcessingContext> createContext)
    {
        var context = createContext();

        context.MessageId = "custom message ID";
        context.ReplyToAddress = "custom reply address";

        IMessageProcessingContext processingContext = context;
        Assert.Multiple(() =>
        {
            Assert.That(processingContext.MessageHeaders[Headers.MessageId], Is.EqualTo("custom message ID"));
            Assert.That(processingContext.MessageHeaders[Headers.ReplyToAddress], Is.EqualTo("custom reply address"));
        });
    }

    [TestCaseSource(nameof(ProcessingContexts))]
    public void ChangingHeadersUpdatesProperties(Func<TestableMessageProcessingContext> createContext)
    {
        var context = createContext();

        context.MessageHeaders[Headers.MessageId] = "custom message ID";
        context.MessageHeaders[Headers.ReplyToAddress] = "custom reply address";

        IMessageProcessingContext processingContext = context;
        Assert.Multiple(() =>
        {
            Assert.That(processingContext.MessageId, Is.EqualTo("custom message ID"));
            Assert.That(processingContext.ReplyToAddress, Is.EqualTo("custom reply address"));
        });

        context.MessageHeaders.Clear();

        Assert.Multiple(() =>
        {
            Assert.That(processingContext.MessageId, Is.Null);
            Assert.That(processingContext.ReplyToAddress, Is.Null);
            Assert.That(processingContext.MessageHeaders, Is.Empty);
        });
    }

    [TestCaseSource(nameof(ProcessingContexts))]
    public void ReplacingHeadersUpdatesProperties(Func<TestableMessageProcessingContext> createContext)
    {
        var context = createContext();
        var headers = new Dictionary<string, string>
        {
            [Headers.MessageId] = "replacement message ID",
            [Headers.ReplyToAddress] = "replacement reply address"
        };

        context.MessageHeaders = headers;

        Assert.Multiple(() =>
        {
            Assert.That(context.MessageHeaders, Is.SameAs(headers));
            Assert.That(context.MessageId, Is.EqualTo("replacement message ID"));
            Assert.That(context.ReplyToAddress, Is.EqualTo("replacement reply address"));
        });

        headers[Headers.MessageId] = "updated message ID";
        Assert.That(context.MessageId, Is.EqualTo("updated message ID"));
    }

    [TestCaseSource(nameof(ProcessingContexts))]
    public void AcceptsOtherDictionaryImplementations(Func<TestableMessageProcessingContext> createContext)
    {
        var context = createContext();
        var headers = new SortedDictionary<string, string>
        {
            [Headers.MessageId] = "replacement message ID",
            [Headers.ReplyToAddress] = "replacement reply address"
        };

        context.MessageHeaders = headers;

        Assert.Multiple(() =>
        {
            Assert.That(context.MessageHeaders, Is.EquivalentTo(headers));
            Assert.That(context.MessageId, Is.EqualTo("replacement message ID"));
            Assert.That(context.ReplyToAddress, Is.EqualTo("replacement reply address"));
        });

        context.MessageId = "updated message ID";
        Assert.That(context.MessageHeaders[Headers.MessageId], Is.EqualTo("updated message ID"));
    }

    [Test]
    public void IncomingHeaderViewsShareDefaultDictionary()
    {
        var invoke = new TestableInvokeHandlerContext();
        var handler = new TestableMessageHandlerContext();
        var logical = new TestableIncomingLogicalMessageContext();
        var physical = new TestableIncomingPhysicalMessageContext();

        Assert.Multiple(() =>
        {
            Assert.That(((IInvokeHandlerContext)invoke).Headers, Is.SameAs(invoke.MessageHeaders));
            Assert.That(((IInvokeHandlerContext)handler).Headers, Is.SameAs(handler.MessageHeaders));
            Assert.That(((IIncomingLogicalMessageContext)logical).Headers, Is.SameAs(logical.MessageHeaders));
            Assert.That(((IIncomingPhysicalMessageContext)physical).Message.Headers, Is.SameAs(physical.MessageHeaders));
            Assert.That(physical.Message.MessageId, Is.EqualTo(physical.MessageId));
        });

        invoke.Headers[Headers.ConversationId] = "custom conversation";
        logical.Headers[Headers.ConversationId] = "custom conversation";
        physical.Message.Headers[Headers.ConversationId] = "custom conversation";

        Assert.Multiple(() =>
        {
            Assert.That(((IMessageProcessingContext)invoke).MessageHeaders[Headers.ConversationId], Is.EqualTo("custom conversation"));
            Assert.That(((IMessageProcessingContext)logical).MessageHeaders[Headers.ConversationId], Is.EqualTo("custom conversation"));
            Assert.That(((IMessageProcessingContext)physical).MessageHeaders[Headers.ConversationId], Is.EqualTo("custom conversation"));
        });
    }

    [Test]
    public void ReplacingLogicalOrHandlerHeadersUpdatesAllViews()
    {
        var invoke = new TestableInvokeHandlerContext();
        var handler = new TestableMessageHandlerContext();
        var logical = new TestableIncomingLogicalMessageContext();
        var headers = new Dictionary<string, string> { [Headers.MessageId] = "replacement message ID" };

        invoke.Headers = headers;
        handler.Headers = headers;
        logical.Headers = headers;

        Assert.Multiple(() =>
        {
            Assert.That(invoke.MessageHeaders, Is.SameAs(headers));
            Assert.That(handler.MessageHeaders, Is.SameAs(headers));
            Assert.That(logical.MessageHeaders, Is.SameAs(headers));
            Assert.That(invoke.MessageId, Is.EqualTo("replacement message ID"));
            Assert.That(handler.MessageId, Is.EqualTo("replacement message ID"));
            Assert.That(logical.MessageId, Is.EqualTo("replacement message ID"));
        });

        var replacement = new Dictionary<string, string>();
        invoke.MessageHeaders = replacement;
        handler.MessageHeaders = replacement;
        logical.MessageHeaders = replacement;

        Assert.Multiple(() =>
        {
            Assert.That(invoke.Headers, Is.SameAs(replacement));
            Assert.That(handler.Headers, Is.SameAs(replacement));
            Assert.That(logical.Headers, Is.SameAs(replacement));
        });
    }

    [Test]
    public void ReplacingPhysicalMessageUpdatesAllViews()
    {
        var context = new TestableIncomingPhysicalMessageContext();
        var headers = new Dictionary<string, string>
        {
            [Headers.MessageId] = "replacement message ID",
            [Headers.ReplyToAddress] = "replacement reply address"
        };
        var message = new IncomingMessage("native message ID", headers, ReadOnlyMemory<byte>.Empty);

        context.Message = message;

        Assert.Multiple(() =>
        {
            Assert.That(context.Message, Is.SameAs(message));
            Assert.That(context.MessageHeaders, Is.SameAs(headers));
            Assert.That(context.MessageId, Is.EqualTo("replacement message ID"));
            Assert.That(context.ReplyToAddress, Is.EqualTo("replacement reply address"));
        });

        context.MessageHeaders = new Dictionary<string, string> { [Headers.MessageId] = "updated message ID" };

        Assert.Multiple(() =>
        {
            Assert.That(context.Message.Headers, Is.SameAs(context.MessageHeaders));
            Assert.That(context.Message.MessageId, Is.EqualTo("updated message ID"));
            Assert.That(context.Message.NativeMessageId, Is.EqualTo("native message ID"));
        });
    }

    [Test]
    public void WrappedMessageIdsMatchHeaders()
    {
        var received = new TestableTransportReceiveContext().Message;
        var failed = new TestableRecoverabilityContext().FailedMessage;

        Assert.Multiple(() =>
        {
            Assert.That(received.MessageId, Is.EqualTo(received.Headers[Headers.MessageId]));
            Assert.That(failed.MessageId, Is.EqualTo(failed.Headers[Headers.MessageId]));
        });
    }

    [Test]
    public void UpdatingPhysicalBodyPreservesHeaders()
    {
        var context = new TestableIncomingPhysicalMessageContext();
        var messageId = context.Message.MessageId;
        var headers = context.Message.Headers;
        byte[] body = [1, 2, 3];
        headers[Headers.ConversationId] = "custom conversation";

        context.UpdateMessage(body);

        Assert.Multiple(() =>
        {
            Assert.That(context.Message.MessageId, Is.EqualTo(messageId));
            Assert.That(context.Message.Headers, Is.SameAs(headers));
            Assert.That(context.Message.Headers[Headers.ConversationId], Is.EqualTo("custom conversation"));
            Assert.That(context.Message.Body.ToArray(), Is.EqualTo(body));
        });
    }

    static IEnumerable<TestCaseData> HeaderSources()
    {
        yield return Source(nameof(TestableMessageProcessingContext), () => new TestableMessageProcessingContext().MessageHeaders);
        yield return Source(nameof(TestableInvokeHandlerContext), () => new TestableInvokeHandlerContext().Headers);
        yield return Source(nameof(TestableMessageHandlerContext), () => new TestableMessageHandlerContext().Headers);
        yield return Source(nameof(TestableIncomingLogicalMessageContext), () => new TestableIncomingLogicalMessageContext().Headers);
        yield return Source(nameof(TestableIncomingPhysicalMessageContext), () => new TestableIncomingPhysicalMessageContext().Message.Headers);
        yield return Source(nameof(TestableTransportReceiveContext), () => new TestableTransportReceiveContext().Message.Headers);
        yield return Source(nameof(TestableRecoverabilityContext), () => new TestableRecoverabilityContext().FailedMessage.Headers);
    }

    static TestCaseData Source(string contextName, Func<IDictionary<string, string>> createHeaders) =>
        new TestCaseData(createHeaders).SetArgDisplayNames(contextName);

    static IEnumerable<TestCaseData> ProcessingContexts()
    {
        yield return Context(() => new TestableMessageProcessingContext());
        yield return Context(() => new TestableInvokeHandlerContext());
        yield return Context(() => new TestableMessageHandlerContext());
        yield return Context(() => new TestableIncomingLogicalMessageContext());
        yield return Context(() => new TestableIncomingPhysicalMessageContext());
    }

    static TestCaseData Context<TContext>(Func<TContext> createContext) where TContext : TestableMessageProcessingContext =>
        new TestCaseData(new Func<TestableMessageProcessingContext>(() => createContext())).SetArgDisplayNames(typeof(TContext).Name);
}
