namespace NServiceBus.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NServiceBus.Extensibility;
    using NServiceBus.MessageInterfaces.MessageMapper.Reflection;
    using NServiceBus.Persistence;
    using NServiceBus.Testing.ExpectedInvocations;

    internal class TestableMessageHandlerContext : IMessageHandlerContext
    {
        IMessageCreator messageCreator = new MessageMapper();

        public List<InvokedMessage> SentMessages { get; } = new List<InvokedMessage>();

        public List<InvokedMessage> PublishedMessages { get; } = new List<InvokedMessage>();
        
        public IList<InvokedMessage> RepliedMessages { get; set; } = new List<InvokedMessage>();

        IList<ActualInvocation> actualInvocations = new List<ActualInvocation>();
        
        public IDictionary<string, string> IncomingHeaders { get; } = new Dictionary<string, string>();

        public IList<ExpectedInvocation> ExpectedInvocations { get; } = new List<ExpectedInvocation>();

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

        public Task Subscribe(Type eventType, SubscribeOptions options)
        {
            throw new NotImplementedException();
        }

        public Task Unsubscribe(Type eventType, UnsubscribeOptions options)
        {
            throw new NotImplementedException();
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
            actualInvocations.Add(new ForwardCurrentMessageToInvocation
            {
                Value = destination
            });
            return Task.FromResult(0);
        }

       
        public Task HandleCurrentMessageLater()
        {
            throw new NotImplementedException();
        }

        public void DoNotContinueDispatchingCurrentMessageToHandlers()
        {
            actualInvocations.Add(new DoNotContinueDispatchingCurrentMessageToHandlersInvocation<object>());
        }

        public SynchronizedStorageSession SynchronizedStorageSession { get; }

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
                actualInvocations.Clear();
            }
        }

        void ProcessInvocation(Type genericType, object message)
        {
            ProcessInvocation(genericType, new Dictionary<string, object>(), message);
        }

        void ProcessInvocation(Type genericType, Dictionary<string, object> others, object message)
        {
            var messageType = GetMessageType(message);
            var invocationType = genericType.MakeGenericType(messageType);
            ProcessInvocationWithBuiltType(invocationType, others, message);
        }

        void ProcessInvocation<K>(Type dualGenericType, Dictionary<string, object> others, object message)
        {
            var invocationType = dualGenericType.MakeGenericType(GetMessageType(message), typeof(K));
            ProcessInvocationWithBuiltType(invocationType, others, message);
        }

        void ProcessInvocationWithBuiltType(Type builtType, Dictionary<string, object> others, object message)
        {
            if (message == null)
            {
                throw new NullReferenceException("message is null.");
            }

            var invocation = Activator.CreateInstance(builtType) as ActualInvocation;

            builtType.GetProperty("Message").SetValue(invocation, message, null);

            foreach (var kv in others)
            {
                builtType.GetProperty(kv.Key).SetValue(invocation, kv.Value, null);
            }

            actualInvocations.Add(invocation);
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
            RepliedMessages.Clear();
            SentMessages.Clear();
            PublishedMessages.Clear();
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