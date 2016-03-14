namespace NServiceBus.Testing.Tests.Saga
{
    using System.Threading.Tasks;
    using NUnit.Framework;

    [TestFixture]
    public class ExpectReplyTests
    {
        [Test]
        public void SagaThatDoesAReply()
        {
            Test.Saga<ReplySaga>()
                .ExpectReply<MyReply>(reply => reply != null)
                .When((s, c) => s.Handle(new MyRequest(), c));
        }

        public class ReplySaga : NServiceBus.Saga<ReplySaga.SagaThatDoesAReplyData>,
        IHandleMessages<MyRequest>
        {
            public Task Handle(MyRequest message, IMessageHandlerContext context)
            {
                return context.Reply(new MyReply());
            }

            protected override void ConfigureHowToFindSaga(SagaPropertyMapper<SagaThatDoesAReplyData> mapper)
            {
            }

            public class SagaThatDoesAReplyData : ContainSagaData
            {
            }
        }
    }
}