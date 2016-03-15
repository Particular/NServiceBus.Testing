namespace NServiceBus.Testing.Tests.Saga
{
    using NUnit.Framework;

    [TestFixture]
    public class ExpectForwardCurrentMessageToTests
    {
        [Test]
        public void ShouldFailExpectForwardCurrentMessageToIfMessageForwardedToUnexpectedDestination()
        {
            Assert.Throws<ExpectationException>(() => Test.Saga<MySaga>()
                .ExpectForwardCurrentMessageTo(dest => dest == "expectedDestination")
                .When((s, c) => s.Handle(new StartsSaga(), c)));
        }

        [Test]
        public void ShouldPassExpectForwardCurrentMessageToIfMessageForwardedToExpectedDestination()
        {
            Test.Saga<MySaga>()
                .ExpectForwardCurrentMessageTo(dest => dest == "forwardingDestination")
                .When((s, c) => s.Handle(new StartsSaga(), c));
        }

        [Test]
        public void ShouldFailExpectNotForwardCurrentMessageToIfMessageForwardedToExpectedDestination()
        {
            Assert.Throws<ExpectationException>(() => Test.Saga<MySaga>()
                .ExpectNotForwardCurrentMessageTo(dest => dest == "forwardingDestination")
                .When((s, c) => s.Handle(new StartsSaga(), c)));
        }

        [Test]
        public void ShouldPassExpectNotForwardCurrentMessageToIfMessageForwardedToUnexpectedDestination()
        {
            Test.Saga<MySaga>()
                .ExpectNotForwardCurrentMessageTo(dest => dest == "expectedDestination")
                .When((s, c) => s.Handle(new StartsSaga(), c));
        }
    }
}