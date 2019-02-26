namespace NServiceBus.Testing.Tests.API
{
    using NUnit.Framework;
    using Particular.Approvals;
    using PublicApiGenerator;

    [TestFixture]
    public class APIApprovals
    {
        [Test]
        public void ApproveTesting()
        {
            var publicApi = ApiGenerator.GeneratePublicApi(typeof(Test).Assembly);
            Approver.Verify(publicApi);
        }
    }
}