namespace NServiceBus.Testing.Tests
{
    using NUnit.Framework;

    [TestFixture]
    public class Issue20 
    {
        public class TestInitialization
        {
            [Test]
            public void ShouldNotThrow()
            {
                Assert.DoesNotThrow(() => Test.Initialize()); 
            }
        }
    }
}