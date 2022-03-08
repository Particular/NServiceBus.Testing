namespace NServiceBus.Testing.Tests.Sagas
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class HeaderMappings
    {
        [Test]
        public async Task TestHeaderMappings()
        {
            var testableSaga = new TestableSaga<HeaderSaga, HeaderSagaData>();
            var correlationId = Guid.NewGuid().ToString().Substring(0, 8);


            var result = await testableSaga.Process(new HeaderMessage(), messageHeaders: new Dictionary<string, string> { { "X-My-Correlation-Id", correlationId } });

            Assert.That(result.Completed, Is.False);
            Assert.That(result.SagaDataSnapshot.CorrId, Is.EqualTo(correlationId));
            Assert.That(result.SagaDataSnapshot.HeaderMessageReceived, Is.True);
        }

        public class HeaderSaga : NServiceBus.Saga<HeaderSagaData>,
            IAmStartedByMessages<HeaderMessage>
        {
            protected override void ConfigureHowToFindSaga(SagaPropertyMapper<HeaderSagaData> mapper)
            {
                mapper.MapSaga(saga => saga.CorrId)
                    .ToMessageHeader<HeaderMessage>("X-My-Correlation-Id");
            }

            public Task Handle(HeaderMessage message, IMessageHandlerContext context)
            {
                Data.HeaderMessageReceived = true;
                return Task.FromResult(false);
            }
        }

        public class HeaderSagaData : ContainSagaData
        {
            public string CorrId { get; set; }
            public bool HeaderMessageReceived { get; set; }
        }

        public class HeaderMessage : ICommand { }
    }
}
