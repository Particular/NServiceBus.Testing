namespace NServiceBus.Testing.Tests
{
    using NUnit.Framework;

    public abstract class BaseTests
    {
        [TestFixtureSetUp]
        public void TestFixtureSetUp()
        {
            Test.Initialize();
        }
    }
}