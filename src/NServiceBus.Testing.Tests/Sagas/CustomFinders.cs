namespace NServiceBus.Testing.Tests.Sagas;

using System;
using System.Threading;
using System.Threading.Tasks;
using Extensibility;
using NServiceBus.Sagas;
using NUnit.Framework;
using Persistence;

[TestFixture]
public class CustomFinders
{
    [Test]
    public async Task TestFinderMappings()
    {
        var testableSaga = new TestableSaga<FinderSaga, FinderSaga.FinderSagaData>();

        testableSaga.MockFinder<StartMessageFinder, StartMessage>(_ => null);

        var finderMessage = new StartMessage
        {
            PartA = Guid.NewGuid().ToString().Substring(0, 8),
            PartB = Guid.NewGuid().ToString().Substring(0, 8)
        };

        var result = await testableSaga.Handle(finderMessage);

        Assert.Multiple(() =>
        {
            Assert.That(result.Completed, Is.False);
            Assert.That(result.SagaDataSnapshot.CorrId, Is.EqualTo(finderMessage.PartA + finderMessage.PartB));
            Assert.That(result.SagaDataSnapshot.MessageReceived, Is.True);
        });
    }

    class StartMessageFinder : ISagaFinder<FinderSaga.FinderSagaData, StartMessage>
    {
        public Task<FinderSaga.FinderSagaData> FindBy(StartMessage message, ISynchronizedStorageSession storageSession, IReadOnlyContextBag context, CancellationToken cancellationToken = default) => throw new NotImplementedException();
    }

    public class FinderSaga : Saga<FinderSaga.FinderSagaData>,
        IAmStartedByMessages<StartMessage>
    {
        public class FinderSagaData : ContainSagaData
        {
            public string CorrId { get; set; }
            public bool MessageReceived { get; set; }
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<FinderSagaData> mapper) => mapper.ConfigureFinderMapping<StartMessage, StartMessageFinder>();

        public Task Handle(StartMessage message, IMessageHandlerContext context)
        {
            Data.MessageReceived = true;
            return Task.CompletedTask;
        }
    }

    public class StartMessage : ICommand
    {
        public string PartA { get; set; }
        public string PartB { get; set; }
    }
}