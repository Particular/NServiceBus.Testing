#if NETFRAMEWORK
namespace NServiceBus.Testing
{
    using System.Collections.Concurrent;

    static class NetFrameworkPolyfillExtensions
    {
        public static void Clear<T>(this ConcurrentQueue<T> queue)
        {
            while (queue.TryDequeue(out _))
            {
            }
        }
    }
}
#endif