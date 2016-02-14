namespace NServiceBus.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NServiceBus.Extensibility;
    using NServiceBus.Persistence;
    using NServiceBus.Testing.ExpectedInvocations;

    internal class TestableMessageHandlerContext : IMessageHandlerContext
    {
        public List<InvokedMessage> SentMessages { get; } = new List<InvokedMessage>();

        public List<InvokedMessage> PublishedMessages { get; } = new List<InvokedMessage>();

        public IList<InvokedMessage> RepliedMessages { get; set; } = new List<InvokedMessage>();

        public IList<string> ForwardedMessages { get; set; } = new List<string>();

        public IDictionary<string, string> IncomingHeaders { get; } = new Dictionary<string, string>();

        public IList<ExpectInvocation> ExpectedInvocations { get; } = new List<ExpectInvocation>();

        public string MessageId { get; }

        public string ReplyToAddress { get; }

        public IReadOnlyDictionary<string, string> MessageHeaders { get; }

        public ContextBag Extensions { get; } = new ContextBag();

        public Task Send(object message, SendOptions options)
        {
            SentMessages.Add(new InvokedMessage(message, options));
            return Task.FromResult(0);
        }

        public Task Send<T>(Action<T> messageConstructor, SendOptions options)
        {
            return Send(messageCreator.CreateInstance(messageConstructor), options);
        }

        public Task Publish(object message, PublishOptions options)
        {
            PublishedMessages.Add(new InvokedMessage(message, options));
            return Task.FromResult(0);
        }

        public Task Publish<T>(Action<T> messageConstructor, PublishOptions publishOptions)
        {
            return Publish(messageCreator.CreateInstance(messageConstructor), publishOptions);
        }

        public Task Reply(object message, ReplyOptions options)
        {
            RepliedMessages.Add(new InvokedMessage(message, options));
            return Task.FromResult(0);
        }

        public Task Reply<T>(Action<T> messageConstructor, ReplyOptions options)
        {
            return Reply(messageCreator.CreateInstance(messageConstructor), options);
        }

        public Task ForwardCurrentMessageTo(string destination)
        {
            ForwardedMessages.Add(destination);
            return Task.FromResult(0);
        }

        public Task HandleCurrentMessageLater()
        {
            throw new NotImplementedException();
        }

        public void DoNotContinueDispatchingCurrentMessageToHandlers()
        {
            throw new NotImplementedException();
        }

        public SynchronizedStorageSession SynchronizedStorageSession { get; }

        public Task Subscribe(Type eventType, SubscribeOptions options)
        {
            throw new NotImplementedException();
        }

        public Task Unsubscribe(Type eventType, UnsubscribeOptions options)
        {
            throw new NotImplementedException();
        }

        public void Validate()
        {
            try
            {
                foreach (var e in ExpectedInvocations)
                {
                    e.Validate(this);
                }
            }
            finally
            {
                Clear();
            }
        }


        Type GetMessageType(object message)
        {
            if (message.GetType().FullName.EndsWith("__impl"))
            {
                var name = message.GetType().FullName.Replace("__impl", "").Replace("\\", "");
                foreach (var i in message.GetType().GetInterfaces())
                {
                    if (i.FullName == name)
                    {
                        return i;
                    }
                }
            }

            return message.GetType();
        }

        public void Clear()
        {
            ExpectedInvocations.Clear();
            //RepliedMessages.Clear();
            //SentMessages.Clear();
            //PublishedMessages.Clear();
        }

        IMessageCreator messageCreator;

        public TestableMessageHandlerContext(IMessageCreator messageCreator)
        {
            this.messageCreator = messageCreator;
        }
    }

    internal class InvokedMessage
    {
        public InvokedMessage(object message, ExtendableOptions sendOptions)
        {
            Message = message;
            SendOptions = sendOptions;
        }

        public object Message { get; private set; }

        public ExtendableOptions SendOptions { get; private set; }
    }
}