using System.Collections.Generic;

namespace ADReplStatus
{
    public sealed class AppState
    {
        private static readonly AppState _instance = new AppState();

        public static AppState Instance => _instance;

        private AppState() { }

        public bool LoggingEnabled { get; set; }

        public bool DarkMode { get; set; }

        public bool ErrorsOnly { get; set; }

        public string LogfileName { get; set; } = string.Empty;

        public string ForestName { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public string Target { get; set; } = string.Empty;

        public volatile bool UseUserDomainController;

        public volatile string UserDomainController = string.Empty;

        public List<ADREPLDC> DCs { get; set; } = new List<ADREPLDC>();
    }
}
