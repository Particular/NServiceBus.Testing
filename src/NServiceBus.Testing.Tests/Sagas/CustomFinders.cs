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
    public async Task TestHeaderMappings()
    {
        var testableSaga = new TestableSaga<FinderSaga, FinderSagaData>();

        var finderMessage = new FinderMessage
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

    public class FinderSaga : Saga<FinderSagaData>,
        IHandleMessages<FinderMessage>
    {
        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<FinderSagaData> mapper) => mapper.ConfigureFinderMapping<FinderMessage, CustomFinder>();

        public Task Handle(FinderMessage message, IMessageHandlerContext context)
        {
            Data.MessageReceived = true;
            return Task.CompletedTask;
        }

        class CustomFinder : ISagaFinder<FinderSagaData, FinderMessage>
        {
            public Task<FinderSagaData> FindBy(FinderMessage message, ISynchronizedStorageSession storageSession, IReadOnlyContextBag context, CancellationToken cancellationToken = default) =>
                Task.FromResult(new FinderSagaData
                {
                    CorrId = message.PartA + message.PartB
                });
        }
    }

    public class FinderSagaData : ContainSagaData
    {
        public string CorrId { get; set; }
        public bool MessageReceived { get; set; }
    }

    public class FinderMessage : ICommand
    {
        public string PartA { get; set; }
        public string PartB { get; set; }
    }
}