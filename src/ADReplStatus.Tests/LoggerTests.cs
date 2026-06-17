using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ADReplStatus.Tests
{
    [TestClass]
    public class LoggerTests
    {
        private string _tempLogFile;

        [TestInitialize]
        public void Setup()
        {
            _tempLogFile = Path.GetTempFileName();
            var state = AppState.Instance;
            state.LogfileName = _tempLogFile;
            state.LoggingEnabled = false;
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(_tempLogFile))
            {
                File.Delete(_tempLogFile);
            }
        }

        [TestMethod]
        public void Log_WhenDisabled_DoesNotWriteToFile()
        {
            AppState.Instance.LoggingEnabled = false;
            File.WriteAllText(_tempLogFile, string.Empty);

            Logger.Log("test message");

            Assert.AreEqual(string.Empty, File.ReadAllText(_tempLogFile));
        }

        [TestMethod]
        public void Log_WhenEnabled_WritesToFile()
        {
            AppState.Instance.LoggingEnabled = true;

            Logger.Log("test message");

            string content = File.ReadAllText(_tempLogFile);
            Assert.IsTrue(content.Contains("test message"));
        }

        [TestMethod]
        public void Log_WhenEnabled_IncludesTimestamp()
        {
            AppState.Instance.LoggingEnabled = true;

            Logger.Log("timestamped");

            string content = File.ReadAllText(_tempLogFile);
            Assert.IsTrue(content.Contains("["));
            Assert.IsTrue(content.Contains("]"));
        }

        [TestMethod]
        public void Log_WhenEnabled_AppendsMultipleMessages()
        {
            AppState.Instance.LoggingEnabled = true;

            Logger.Log("first");
            Logger.Log("second");

            string content = File.ReadAllText(_tempLogFile);
            Assert.IsTrue(content.Contains("first"));
            Assert.IsTrue(content.Contains("second"));
        }
    }
}
