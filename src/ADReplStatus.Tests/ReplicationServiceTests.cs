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

        internal class MockProgressReporter : IProgressReporter
        {
            public int ReportedPercent { get; private set; }
            public string ReportedMessage { get; private set; }
            public bool FallbackAsked { get; private set; }
            public string FallbackDCName { get; private set; }
            public bool FallbackResponse { get; set; } = true;

            public void ReportProgress(int percent, string message)
            {
                ReportedPercent = percent;
                ReportedMessage = message;
            }

            public bool AskFallbackToAutomaticDiscovery(string dcName)
            {
                FallbackAsked = true;
                FallbackDCName = dcName;
                return FallbackResponse;
            }
        }

        [TestClass]
        public class FallbackTests
        {
            [TestInitialize]
            public void Setup()
            {
                var state = AppState.Instance;
                state.ForestName = "contoso.com";
                state.Username = string.Empty;
                state.Password = string.Empty;
                state.UseUserDomainController = false;
                state.UserDomainController = string.Empty;
            }

            [TestMethod]
            public void MockProgressReporter_AskFallbackToAutomaticDiscovery_ReturnsTrueByDefault()
            {
                var reporter = new MockProgressReporter();

                bool result = reporter.AskFallbackToAutomaticDiscovery("DC01.contoso.com");

                Assert.IsTrue(result);
            }

            [TestMethod]
            public void MockProgressReporter_AskFallbackToAutomaticDiscovery_ReturnsFalseWhenSet()
            {
                var reporter = new MockProgressReporter();
                reporter.FallbackResponse = false;

                bool result = reporter.AskFallbackToAutomaticDiscovery("DC01.contoso.com");

                Assert.IsFalse(result);
            }

            [TestMethod]
            public void MockProgressReporter_AskFallbackToAutomaticDiscovery_TracksRequest()
            {
                var reporter = new MockProgressReporter();

                reporter.AskFallbackToAutomaticDiscovery("DC01.contoso.com");

                Assert.IsTrue(reporter.FallbackAsked);
                Assert.AreEqual("DC01.contoso.com", reporter.FallbackDCName);
            }

            [TestMethod]
            public void MockProgressReporter_ReportProgress_TracksPercentAndMessage()
            {
                var reporter = new MockProgressReporter();

                reporter.ReportProgress(50, "Test message");

                Assert.AreEqual(50, reporter.ReportedPercent);
                Assert.AreEqual("Test message", reporter.ReportedMessage);
            }
        }
    }
}
