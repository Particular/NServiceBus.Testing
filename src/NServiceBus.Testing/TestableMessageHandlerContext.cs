namespace NServiceBus.Testing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using NServiceBus.Extensibility;
    using NServiceBus.Persistence;

    internal class TestableMessageHandlerContext : IMessageHandlerContext
    {
        public IDictionary<string, string> IncomingHeaders { get; } = new Dictionary<string, string>();

        public IList<IExpectedInvocation> ExpectedInvocations { get; } = new List<IExpectedInvocation>();
        IList<ActualInvocation> actualInvocations = new List<ActualInvocation>();

        public ContextBag Extensions { get; } = new ContextBag();
        
        public Task Send(object message, SendOptions options)
        {
            throw new NotImplementedException();
        }

        public Task Send<T>(Action<T> messageConstructor, SendOptions options)
        {
            throw new NotImplementedException();
        }

        public Task Publish(object message, PublishOptions options)
        {
            throw new NotImplementedException();
        }

        public Task Publish<T>(Action<T> messageConstructor, PublishOptions publishOptions)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public Task Reply<T>(Action<T> messageConstructor, ReplyOptions options)
        {
            throw new NotImplementedException();
        }

        public Task ForwardCurrentMessageTo(string destination)
        {
            throw new NotImplementedException();
        }

        public string MessageId { get; }
        public string ReplyToAddress { get; }
        public IReadOnlyDictionary<string, string> MessageHeaders { get; }
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
                    e.Validate(actualInvocations.ToArray());
                }
            }
            finally
            {
                actualInvocations.Clear();
            }
        }
    }
}