namespace NServiceBus.Testing.Tests.Saga
{
    using System;
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    class SagaDataTests
    {
        [Test]
        public void ShouldCreateSagaWithSagaData()
        {
            Test.Saga<SagaCreation>(new SagaCreation.CreationData { Property = "property" })
                .ExpectSagaData<SagaCreation.CreationData>(data => data.Property == "property")
                .WhenHandling<SagaCreation.NoChangeMessage>();
        }

        [Test]
        public void ExpectSagaDataShouldPassWhenCheckSucceeds()
        {
            Test.Saga<SagaCreation>(new SagaCreation.CreationData { Property = "property" })
                .ExpectSagaData<SagaCreation.CreationData>(data => data.Property == "newData")
                .WhenHandling<SagaCreation.ChangeMessage>();
        }

        [Test]
        public void ExpectSagaDataShouldThrowExpectationExceptionWhenCheckDoesNotSucceed()
        {
            Assert.Throws<ExpectationException>(() => Test.Saga<SagaCreation>(new SagaCreation.CreationData { Property = "property" })
                .ExpectSagaData<SagaCreation.CreationData>(data => data.Property == "42")
                .WhenHandling<SagaCreation.ChangeMessage>());
        }
    }

    public class SagaCreation : NServiceBus.Saga<SagaCreation.CreationData>,
        IHandleMessages<SagaCreation.NoChangeMessage>,
        IHandleMessages<SagaCreation.ChangeMessage>
    {
        public Task Handle(ChangeMessage message, IMessageHandlerContext context)
        {
            Data.Property = "newData";
            return Task.FromResult(0);
        }

        public Task Handle(NoChangeMessage message, IMessageHandlerContext context)
        {
            return Task.FromResult(0);
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<CreationData> mapper)
        {
        }

        public class CreationData : IContainSagaData
        {
            public string Property { get; set; }
            public Guid Id { get; set; }

            public string Originator { get; set; }

            public string OriginalMessageId { get; set; }
        }

        public class NoChangeMessage
        {
        }

        public class ChangeMessage
        {
        }
    }
}