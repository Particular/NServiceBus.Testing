namespace NServiceBus.Testing.Tests.Contexts
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]

    public class TestContexts
    {
        [Test]
        public async Task RunTestableMessageContext()
        {
            var context = new TestableMessageHandlerContext();

            await context.Send(new Cmd { Number = 1 });
            await context.Send(new Cmd { Number = 2 }, new SendOptions());
            await context.Send("dest", new Cmd { Number = 3 });
            await context.Send<Cmd>(cmd => cmd.Number = 4);
            await context.Send<Cmd>("dest", cmd => cmd.Number = 5);
            await context.Send<Cmd>(cmd => cmd.Number = 6, new SendOptions());

            await context.Publish<Evt>();
            await context.Publish(new Evt { Number = 1 });
            await context.Publish(new Evt { Number = 2 }, new PublishOptions());
            await context.Publish<Evt>(cmd => cmd.Number = 3);
            await context.Publish<Evt>(cmd => cmd.Number = 4, new PublishOptions());

            Assert.That(context.SentMessages.Length == 6);
            string sentNumbers = string.Join(",", context.SentMessages.Select(m => (m.Message as Cmd).Number.ToString()));
            Assert.AreEqual("1,2,3,4,5,6", sentNumbers);

            Assert.That(context.PublishedMessages.Length == 5);
            string publishedNumbers = string.Join(",", context.PublishedMessages.Select(m => (m.Message as Evt).Number.ToString()));
            Assert.AreEqual("0,1,2,3,4", publishedNumbers);
        }

        [Test]
        public async Task RunTestableMessageSession()
        {
            await RunTestableMessageSessionInternal(new TestableMessageSession(), CancellationToken.None);
        }

        [Test]
        public async Task RunTestableEndpointInstance()
        {
            var context = new TestableEndpointInstance();

            await RunTestableMessageSessionInternal(context, CancellationToken.None);

            await context.Stop();
            Assert.AreEqual(true, context.EndpointStopped);
        }

        public static async Task RunTestableMessageSessionInternal<TContext>(TContext context, CancellationToken cancellationToken = default)
            where TContext : TestableMessageSession
        {
            await context.Send(new Cmd { Number = 1 }, cancellationToken);
            await context.Send(new Cmd { Number = 2 }, new SendOptions(), cancellationToken);
            await context.Send("dest", new Cmd { Number = 3 }, cancellationToken);
            await context.Send<Cmd>(cmd => cmd.Number = 4, cancellationToken);
            await context.Send<Cmd>("dest", cmd => cmd.Number = 5, cancellationToken);
            await context.Send<Cmd>(cmd => cmd.Number = 6, new SendOptions(), cancellationToken);

            await context.Publish<Evt>(cancellationToken);
            await context.Publish(new Evt { Number = 1 }, cancellationToken);
            await context.Publish(new Evt { Number = 2 }, new PublishOptions(), cancellationToken);
            await context.Publish<Evt>(cmd => cmd.Number = 3, cancellationToken: cancellationToken);
            await context.Publish<Evt>(cmd => cmd.Number = 4, new PublishOptions(), cancellationToken);

            Assert.That(context.SentMessages.Length == 6);
            string sentNumbers = string.Join(",", context.SentMessages.Select(m => (m.Message as Cmd).Number.ToString()));
            Assert.AreEqual("1,2,3,4,5,6", sentNumbers);

            Assert.That(context.PublishedMessages.Length == 5);
            string publishedNumbers = string.Join(",", context.PublishedMessages.Select(m => (m.Message as Evt).Number.ToString()));
            Assert.AreEqual("0,1,2,3,4", publishedNumbers);
        }

        class Cmd : ICommand, IHaveNumber
        {
            public int Number { get; set; }
        }

        class Evt : IEvent, IHaveNumber
        {
            public int Number { get; set; }
        }

        interface IHaveNumber
        {
            int Number { get; set; }
        }
    }
}
