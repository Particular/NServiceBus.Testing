namespace NServiceBus.Testing
{
    using System;
    using System.Threading.Tasks;
    using Transports;

    class FakeDequer : IPushMessages
    {
        public Task Init(Func<PushContext, Task> pipe, CriticalError criticalError, PushSettings settings)
        {
            return Task.FromResult(0);
        }

        public void Start(PushRuntimeSettings limitations)
        {
        }

        public Task Stop()
        {
            return Task.FromResult(0);
        }
    }
}