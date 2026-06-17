using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ADReplStatus.Tests
{
    [TestClass]
    public class ADREPLDCTests
    {
        [TestMethod]
        public void NewInstance_DiscoveryIssues_DefaultsToFalse()
        {
            var dc = new ADREPLDC();

            Assert.IsFalse(dc.DiscoveryIssues);
        }

        [TestMethod]
        public void NewInstance_ReplicationPartners_IsEmptyList()
        {
            var dc = new ADREPLDC();

            Assert.IsNotNull(dc.ReplicationPartners);
            Assert.AreEqual(0, dc.ReplicationPartners.Count);
        }

        [TestMethod]
        public void Properties_CanBeSetAndRead()
        {
            var dc = new ADREPLDC
            {
                Name = "DC01.contoso.com",
                DomainName = "contoso.com",
                Site = "Default-First-Site-Name",
                IsGC = "True",
                IsRODC = "False",
                DiscoveryIssues = true
            };

            Assert.AreEqual("DC01.contoso.com", dc.Name);
            Assert.AreEqual("contoso.com", dc.DomainName);
            Assert.AreEqual("Default-First-Site-Name", dc.Site);
            Assert.AreEqual("True", dc.IsGC);
            Assert.AreEqual("False", dc.IsRODC);
            Assert.IsTrue(dc.DiscoveryIssues);
        }

        [TestMethod]
        public void UnreachableDC_HasExpectedUnknownValues()
        {
            var dc = new ADREPLDC
            {
                Name = "DC02.contoso.com",
                DomainName = "contoso.com",
                Site = "Unknown",
                IsGC = "Unknown",
                IsRODC = "Unknown",
                DiscoveryIssues = true
            };

            Assert.AreEqual("Unknown", dc.Site);
            Assert.AreEqual("Unknown", dc.IsGC);
            Assert.AreEqual("Unknown", dc.IsRODC);
            Assert.IsTrue(dc.DiscoveryIssues);
        }
    }
}
