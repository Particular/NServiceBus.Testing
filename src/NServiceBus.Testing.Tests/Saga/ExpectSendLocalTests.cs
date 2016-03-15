namespace NServiceBus.Testing.Tests.Saga
{
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class ExpectSendLocalTests
    {
        [Test]
        public void ExpectSendLocal()
        {
            Test.Saga<SendLocalSaga>()
                .ExpectSendLocal<SendLocalSaga.Message>(m => m.Property == "Property")
                .WhenHandling<SendLocalSaga.RequestSendLocal>();
        }

        [Test]
        public void ExpectNotSendLocal()
        {
            Test.Saga<SendLocalSaga>()
                .ExpectNotSendLocal<SendLocalSaga.Message>()
                .WhenHandling<SendLocalSaga.RequestNotSendLocal>();
        }

        public class SendLocalSaga : NServiceBus.Saga<SendLocalSaga.SendLocalSagaData>,
            IHandleMessages<SendLocalSaga.RequestSendLocal>,
            IHandleMessages<SendLocalSaga.RequestNotSendLocal>
        {
            public Task Handle(RequestNotSendLocal message, IMessageHandlerContext context)
            {
                return Task.FromResult(0);
            }

            public Task Handle(RequestSendLocal message, IMessageHandlerContext context)
            {
                return context.SendLocal(new Message());
            }

            protected override void ConfigureHowToFindSaga(SagaPropertyMapper<SendLocalSagaData> mapper)
            {
            }

            public class SendLocalSagaData : ContainSagaData
            {
            }

            public class RequestSendLocal
            {
            }

            public class RequestNotSendLocal
            {
            }

            public class Message
            {
                public string Property => "Property";
            }
        }
    }
}