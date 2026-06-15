using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ADReplStatus.Tests
{
    [TestClass]
    public class ReplicationServiceTests
    {
        [TestMethod]
        public void IsDCReachable_InvalidHost_ReturnsFalse()
        {
            bool result = ReplicationService.IsDCReachable("host.invalid.tld.that.does.not.exist");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsDCReachable_InvalidPort_ReturnsFalse()
        {
            bool result = ReplicationService.IsDCReachable("localhost", 1);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void IsDCReachable_EmptyHostname_ReturnsFalse()
        {
            bool result = ReplicationService.IsDCReachable(string.Empty);

            Assert.IsFalse(result);
        }
    }
}
