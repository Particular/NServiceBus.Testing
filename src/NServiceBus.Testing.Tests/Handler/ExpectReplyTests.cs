namespace NServiceBus.Testing.Tests.Handler
{
    using System.Threading;
    using NServiceBus.Testing.Tests.Saga;
    using NUnit.Framework;

    [TestFixture]
    public class ExpectReplyTests
    {
        [Test]
        public void ShouldPassExpectReplyWhenReypling()
        {
            Test.Handler<ReplyingHandler>()
                .ExpectReply<MyReply>()
                .OnMessage(new MyRequest { ShouldReply = true });
        }

        [Test]
        public void ShouldPassExpectReplyProvidedOptionsToCheck()
        {
            var options = new ReplyOptions();
            ReplyOptions capturedOptions = null;

            Test.Handler<ReplyingHandler>()
                .WithExternalDependencies(handler => handler.OptionsProvider = () => options)
                .ExpectReply<MyReply>((reply, replyOptions) =>
                {
                    capturedOptions = replyOptions;
                    return true;
                })
                .OnMessage(new MyRequest
                {
                    ShouldReply = true
                });

            Assert.AreSame(options, capturedOptions);
        }

        [Test]
        public void ShouldPassExpectNotReplyWhenNotReplying()
        {
            Test.Handler<ReplyingHandler>()
                .ExpectNotReply<MyReply>()
                .OnMessage(new MyRequest { ShouldReply = false });
        }

        [Test]
        public void ShouldPassExpectNotReplyProvidedOptionsToCheck()
        {
            var options = new ReplyOptions();
            ReplyOptions capturedOptions = null;

            Test.Handler<ReplyingHandler>()
                .WithExternalDependencies(handler => handler.OptionsProvider = () => options)
                .ExpectNotReply<MyReply>((reply, replyOptions) =>
                {
                    capturedOptions = replyOptions;
                    return false;
                })
                .OnMessage(new MyRequest
                {
                    ShouldReply = true
                });

            Assert.AreSame(options, capturedOptions);
        }

        [Test]
        public void ReplyShouldBeThreadsafe()
        {
            var counter = 0;

            Assert.Throws<ExpectationException>(() => Test.Handler<ConcurrentHandler>()
                .WithExternalDependencies(h =>
                {
                    h.NumberOfThreads = 100;
                    h.HandlerAction = context => context.Reply<Send1>(m => { });
                })
                .ExpectReply<Send1>(m =>
                {
                    Interlocked.Increment(ref counter);
                    return false;
                })
                .OnMessage<MyCommand>());

            Assert.AreEqual(100, counter);
        }
    }
}