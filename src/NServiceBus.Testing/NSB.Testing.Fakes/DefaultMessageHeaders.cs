namespace NServiceBus.Testing;

using System;
using System.Collections.Generic;

static class DefaultMessageHeaders
{
    public const string MessageId = "00000000-0000-0000-0000-000000000001";
    public const string ReplyToAddress = "reply address";

    public static Dictionary<string, string> Create() => new()
    {
        [Headers.MessageId] = MessageId,
        [Headers.CorrelationId] = "00000000-0000-0000-0000-000000000002",
        [Headers.ConversationId] = "00000000-0000-0000-0000-000000000003",
        [Headers.ReplyToAddress] = ReplyToAddress,
        [Headers.OriginatingEndpoint] = "originating endpoint",
        [Headers.TimeSent] = DateTimeOffsetHelper.ToWireFormattedString(new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero))
    };
}
