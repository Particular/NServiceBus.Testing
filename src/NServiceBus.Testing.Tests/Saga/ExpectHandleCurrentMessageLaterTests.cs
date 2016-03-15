namespace NServiceBus.Testing.Tests.Saga
{
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class ExpectHandleCurrentMessageLaterTests
    {
        [Test]
        public void ExpectHandleCurrentMessageLater()
        {
            Test.Saga<HandleInFutureSaga>()
                .ExpectHandleCurrentMessageLater()
                .WhenHandling<MyRequest>();
        }

        public class HandleInFutureSaga : NServiceBus.Saga<HandleInFutureSaga.HandleInFutureSagaData>,
            IHandleMessages<MyRequest>
        {
            public Task Handle(MyRequest message, IMessageHandlerContext context)
            {
                return context.HandleCurrentMessageLater();
            }

            protected override void ConfigureHowToFindSaga(SagaPropertyMapper<HandleInFutureSagaData> mapper)
            {
            }

            public class HandleInFutureSagaData : ContainSagaData
            {
            }
        }
    }
}