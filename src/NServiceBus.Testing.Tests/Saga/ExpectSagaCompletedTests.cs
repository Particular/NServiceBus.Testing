namespace NServiceBus.Testing.Tests.Saga
{
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class ExpectSagaCompletedTests
    {
        [Test]
        public void ExpectSagaCompletedShouldPassWhenSagaCompleted()
        {
            Test.Saga<TestSaga>()
                .ExpectSagaCompleted()
                .WhenHandling<TestSaga.CompleteSagaMessage>();
        }

        [Test]
        public void ExpectSagaCompletedShouldFailWhenSagaNotCompleted()
        {
            var exception = Assert.Throws<ExpectationException>(() => Test.Saga<TestSaga>()
                .ExpectSagaCompleted()
                .WhenHandling<TestSaga.DoNotCompleteSagaMessage>());

            StringAssert.Contains("Expected saga to be completed but the saga was not completed", exception.Message);
        }

        [Test]
        public void ExpectSagaNotCompletedShouldPassWhenSagaNotCompleted()
        {
            Test.Saga<TestSaga>()
                .ExpectSagaNotCompleted()
                .WhenHandling<TestSaga.DoNotCompleteSagaMessage>();
        }

        [Test]
        public void ExpectSagaNotCompletedShouldFailWhenSagaCompleted()
        {
            var exception = Assert.Throws<ExpectationException>(() => Test.Saga<TestSaga>()
                .ExpectSagaNotCompleted()
                .WhenHandling<TestSaga.CompleteSagaMessage>());

            StringAssert.Contains("Expected saga to not be completed but the saga was completed", exception.Message);
        }
    }

    class TestSaga : NServiceBus.Saga<TestSaga.TestSagaData>, 
        IAmStartedByMessages<TestSaga.CompleteSagaMessage>,
        IAmStartedByMessages<TestSaga.DoNotCompleteSagaMessage>
    {
        public class TestSagaData : ContainSagaData
        {
            public int CorrelationId { get; set; }
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<TestSagaData> mapper)
        {
            mapper.ConfigureMapping<CompleteSagaMessage>(m => m.CorrelationId).ToSaga(s => s.CorrelationId);
            mapper.ConfigureMapping<DoNotCompleteSagaMessage>(m => m.CorrelationId).ToSaga(s => s.CorrelationId);
        }

        public class CompleteSagaMessage : IMessage
        {
            public int CorrelationId { get; set; }
        }

        public class DoNotCompleteSagaMessage : IMessage
        {
            public int CorrelationId { get; set; }
        }

        public Task Handle(CompleteSagaMessage message, IMessageHandlerContext context)
        {
            MarkAsComplete();
            return Task.FromResult(0);
        }

        public Task Handle(DoNotCompleteSagaMessage message, IMessageHandlerContext context)
        {
            return Task.FromResult(0);
        }
    }

    
}