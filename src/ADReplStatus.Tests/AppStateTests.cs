using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ADReplStatus.Tests
{
    [TestClass]
    public class AppStateTests
    {
        [TestInitialize]
        public void Setup()
        {
            var state = AppState.Instance;
            state.LoggingEnabled = false;
            state.DarkMode = false;
            state.ErrorsOnly = false;
            state.LogfileName = string.Empty;
            state.ForestName = string.Empty;
            state.Username = string.Empty;
            state.Password = string.Empty;
            state.Target = string.Empty;
            state.UseUserDomainController = false;
            state.UserDomainController = string.Empty;
            state.DCs.Clear();
        }

        [TestMethod]
        public void Instance_ReturnsSameInstance()
        {
            var first = AppState.Instance;
            var second = AppState.Instance;

            Assert.AreSame(first, second);
        }

        [TestMethod]
        public void Instance_IsNotNull()
        {
            Assert.IsNotNull(AppState.Instance);
        }

        [TestMethod]
        public void DefaultStringProperties_AreEmpty()
        {
            var state = AppState.Instance;

            Assert.AreEqual(string.Empty, state.ForestName);
            Assert.AreEqual(string.Empty, state.Username);
            Assert.AreEqual(string.Empty, state.Password);
            Assert.AreEqual(string.Empty, state.Target);
            Assert.AreEqual(string.Empty, state.LogfileName);
        }

        [TestMethod]
        public void DefaultBoolProperties_AreFalse()
        {
            var state = AppState.Instance;

            Assert.IsFalse(state.LoggingEnabled);
            Assert.IsFalse(state.DarkMode);
            Assert.IsFalse(state.ErrorsOnly);
            Assert.IsFalse(state.UseUserDomainController);
        }

        [TestMethod]
        public void DCs_DefaultsToEmptyList()
        {
            var state = AppState.Instance;

            Assert.IsNotNull(state.DCs);
            Assert.AreEqual(0, state.DCs.Count);
        }

        [TestMethod]
        public void Properties_CanBeSetAndRead()
        {
            var state = AppState.Instance;

            state.ForestName = "contoso.com";
            state.Username = "admin";
            state.Password = "secret";
            state.DarkMode = true;
            state.LoggingEnabled = true;
            state.ErrorsOnly = true;

            Assert.AreEqual("contoso.com", state.ForestName);
            Assert.AreEqual("admin", state.Username);
            Assert.AreEqual("secret", state.Password);
            Assert.IsTrue(state.DarkMode);
            Assert.IsTrue(state.LoggingEnabled);
            Assert.IsTrue(state.ErrorsOnly);
        }

        [TestMethod]
        public void DCs_CanAddItems()
        {
            var state = AppState.Instance;
            var dc = new ADREPLDC { Name = "DC01.contoso.com", DomainName = "contoso.com" };

            state.DCs.Add(dc);

            Assert.AreEqual(1, state.DCs.Count);
            Assert.AreEqual("DC01.contoso.com", state.DCs[0].Name);
        }
    }
}
